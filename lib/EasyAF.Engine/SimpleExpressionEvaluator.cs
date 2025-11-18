using System;
using System.Collections.Generic;
using System.Globalization;

namespace EasyAF.Engine
{
    public static class SimpleExpressionEvaluator
    {
        // Supports + - * / and parentheses, numbers in invariant culture.
        public static double Evaluate(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr)) return 0;
            var tokens = Tokenize(expr);
            int idx = 0;
            double ParseExpression()
            {
                double val = ParseTerm();
                while (idx < tokens.Count)
                {
                    var t = tokens[idx];
                    if (t == "+" || t == "-")
                    { idx++; var rhs = ParseTerm(); val = t == "+" ? val + rhs : val - rhs; }
                    else break;
                }
                return val;
            }
            double ParseTerm()
            {
                double val = ParseFactor();
                while (idx < tokens.Count)
                {
                    var t = tokens[idx];
                    if (t == "*" || t == "/")
                    { idx++; var rhs = ParseFactor(); val = t == "*" ? val * rhs : (rhs == 0 ? 0 : val / rhs); }
                    else break;
                }
                return val;
            }
            double ParseFactor()
            {
                if (idx >= tokens.Count) return 0;
                var t = tokens[idx++];
                if (t == "+") return ParseFactor();
                if (t == "-") return -ParseFactor();
                if (t == "(") { var v = ParseExpression(); if (idx < tokens.Count && tokens[idx] == ")") idx++; return v; }
                if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return d;
                return 0;
            }
            var result = ParseExpression();
            return result;
        }

        private static List<string> Tokenize(string s)
        {
            var list = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if ("+-*/()".IndexOf(c) >= 0) { list.Add(c.ToString()); i++; continue; }
                int start = i;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '+' || s[i] == '-'))
                {
                    if ((s[i] == '+' || s[i] == '-') && i > start && (s[i - 1] != 'e' && s[i - 1] != 'E')) break;
                    i++;
                }
                list.Add(s.Substring(start, i - start));
            }
            return list;
        }
    }
}
