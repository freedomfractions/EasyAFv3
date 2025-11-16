using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EasyAF.Engine
{
    internal static class ExpressionCompiler
    {
        private static readonly ConcurrentDictionary<string, CompiledNumericExpression> _cache = new(StringComparer.OrdinalIgnoreCase);

        public static CompiledNumericExpression GetOrAdd(string expression, string? numberFormat)
        {
            var key = expression + "|" + numberFormat;
            return _cache.GetOrAdd(key, _ => Compile(expression, numberFormat));
        }

        private static CompiledNumericExpression Compile(string expr, string? numberFormat)
        {
            if (string.IsNullOrWhiteSpace(expr)) return new CompiledNumericExpression(new List<string>(), new List<Token>(), numberFormat);
            // Collect distinct property paths inside { }
            var propOrder = new List<string>();
            foreach (Match m in Regex.Matches(expr, "{([^}]+)}"))
            {
                var prop = m.Groups[1].Value.Trim();
                bool exists = propOrder.Exists(p => p.Equals(prop, StringComparison.OrdinalIgnoreCase));
                if (!exists) propOrder.Add(prop);
            }
            // Replace tokens with @index markers (case-insensitive)
            string normalized = expr;
            for (int i = 0; i < propOrder.Count; i++)
            {
                var pattern = "\\{" + Regex.Escape(propOrder[i]) + "\\}"; // literal {Prop}
                normalized = Regex.Replace(normalized, pattern, "@" + i, RegexOptions.IgnoreCase);
            }
            var rpn = ToRpn(normalized);
            return new CompiledNumericExpression(propOrder, rpn, numberFormat);
        }

        private static List<Token> ToRpn(string expr)
        {
            var output = new List<Token>();
            var ops = new Stack<char>();
            int i = 0;
            while (i < expr.Length)
            {
                char c = expr[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '@')
                {
                    i++; int start = i; while (i < expr.Length && char.IsDigit(expr[i])) i++;
                    var numStr = expr.Substring(start, i - start);
                    if (!int.TryParse(numStr, out var index)) index = 0;
                    output.Add(Token.Variable(index));
                    continue;
                }
                if (char.IsDigit(c) || c == '.')
                {
                    int start = i; i++;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i]=='.' || expr[i]=='e' || expr[i]=='E' || expr[i]=='+' || expr[i]=='-'))
                    {
                        if ((expr[i] == '+' || expr[i] == '-') && !(expr[i-1]=='e' || expr[i-1]=='E')) break;
                        i++;
                    }
                    var lit = expr.Substring(start, i - start);
                    if (double.TryParse(lit, NumberStyles.Float, CultureInfo.InvariantCulture, out var dv)) output.Add(Token.Number(dv)); else output.Add(Token.Number(0));
                    continue;
                }
                if (IsOperator(c))
                {
                    while (ops.Count > 0 && IsOperator(ops.Peek()) && Precedence(ops.Peek()) >= Precedence(c))
                        output.Add(Token.Operator(ops.Pop()));
                    ops.Push(c); i++; continue;
                }
                if (c == '(') { ops.Push(c); i++; continue; }
                if (c == ')')
                {
                    while (ops.Count > 0 && ops.Peek() != '(') output.Add(Token.Operator(ops.Pop()));
                    if (ops.Count > 0 && ops.Peek() == '(') ops.Pop();
                    i++; continue;
                }
                // unknown char skip
                i++;
            }
            while (ops.Count > 0) output.Add(Token.Operator(ops.Pop()));
            return output;
        }

        private static bool IsOperator(char c) => c is '+' or '-' or '*' or '/';
        private static int Precedence(char c) => c is '+' or '-' ? 1 : 2;
    }

    internal enum TokenType { Number, Variable, Operator }
    internal readonly struct Token
    {
        public TokenType Type { get; }
        public double NumberValue { get; }
        public int VariableIndex { get; }
        public char OperatorChar { get; }
        private Token(TokenType t, double n, int v, char op) { Type=t; NumberValue=n; VariableIndex=v; OperatorChar=op; }
        public static Token Number(double v) => new(TokenType.Number, v, -1, '\0');
        public static Token Variable(int idx) => new(TokenType.Variable, 0, idx, '\0');
        public static Token Operator(char op) => new(TokenType.Operator, 0, -1, op);
    }

    internal sealed class CompiledNumericExpression
    {
        private readonly List<string> _propertyPaths;
        private readonly List<Token> _rpn;
        private readonly string? _numberFormat;
        public CompiledNumericExpression(List<string> propertyPaths, List<Token> rpn, string? numberFormat)
        { _propertyPaths = propertyPaths; _rpn = rpn; _numberFormat = numberFormat; }

        public string Render(object row)
        {
            try
            {
                var stack = new Stack<double>();
                foreach (var t in _rpn)
                {
                    switch (t.Type)
                    {
                        case TokenType.Number:
                            stack.Push(t.NumberValue); break;
                        case TokenType.Variable:
                            double val = 0;
                            if (t.VariableIndex >=0 && t.VariableIndex < _propertyPaths.Count)
                            {
                                var raw = EasyAFEngine.EvaluatePropertyPath(row, _propertyPaths[t.VariableIndex]);
                                if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out val)) val = 0;
                            }
                            stack.Push(val); break;
                        case TokenType.Operator:
                            double b = stack.Count>0? stack.Pop():0; double a = stack.Count>0? stack.Pop():0;
                            double r = t.OperatorChar switch
                            {
                                '+' => a + b,
                                '-' => a - b,
                                '*' => a * b,
                                '/' => (b == 0 ? 0 : a / b),
                                _ => 0
                            };
                            stack.Push(r); break;
                    }
                }
                double result = stack.Count>0? stack.Pop():0;
                if (_numberFormat != null && double.IsFinite(result)) return result.ToString(_numberFormat, CultureInfo.InvariantCulture);
                return result.ToString("0.###", CultureInfo.InvariantCulture);
            }
            catch { return string.Empty; }
        }
    }
}
