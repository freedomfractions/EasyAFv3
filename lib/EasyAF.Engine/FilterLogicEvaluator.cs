using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyAF.Engine
{
    /// <summary>
    /// Evaluates advanced filter logic expressions like "(1 | 2) & 3" or "!1 & (2 | 3)".
    /// Supports operators: & (AND), | (OR), ! (NOT), and parentheses for grouping.
    /// Numbers represent filter rule indices (1-based).
    /// </summary>
    public static class FilterLogicEvaluator
    {
        /// <summary>
        /// Evaluates an advanced filter logic expression against a list of filter evaluation results.
        /// </summary>
        /// <param name="expression">Expression like "(1 | 2) & 3" where numbers are 1-based filter indices</param>
        /// <param name="filterResults">Array of boolean results from evaluating each filter (1-based indexing)</param>
        /// <returns>True if the expression evaluates to true, false otherwise</returns>
        /// <exception cref="ArgumentException">If expression is invalid or references non-existent filters</exception>
        public static bool Evaluate(string expression, bool[] filterResults)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return true; // Empty expression = no filtering

            // Parse and evaluate the expression
            var tokens = Tokenize(expression);
            var postfix = InfixToPostfix(tokens);
            return EvaluatePostfix(postfix, filterResults);
        }

        /// <summary>
        /// Tokenizes the expression into a list of tokens (numbers, operators, parentheses).
        /// </summary>
        private static List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            var cleaned = Regex.Replace(expression, @"\s+", ""); // Remove whitespace

            for (int i = 0; i < cleaned.Length; i++)
            {
                char c = cleaned[i];

                if (char.IsDigit(c))
                {
                    // Parse multi-digit number
                    int start = i;
                    while (i < cleaned.Length && char.IsDigit(cleaned[i]))
                        i++;
                    
                    var numStr = cleaned.Substring(start, i - start);
                    tokens.Add(new Token { Type = TokenType.Number, Value = int.Parse(numStr) });
                    i--; // Back up one since the loop will increment
                }
                else if (c == '&')
                {
                    tokens.Add(new Token { Type = TokenType.And });
                }
                else if (c == '|')
                {
                    tokens.Add(new Token { Type = TokenType.Or });
                }
                else if (c == '!')
                {
                    tokens.Add(new Token { Type = TokenType.Not });
                }
                else if (c == '(')
                {
                    tokens.Add(new Token { Type = TokenType.LeftParen });
                }
                else if (c == ')')
                {
                    tokens.Add(new Token { Type = TokenType.RightParen });
                }
                else
                {
                    throw new ArgumentException($"Invalid character '{c}' in filter expression at position {i}");
                }
            }

            return tokens;
        }

        /// <summary>
        /// Converts infix notation to postfix (Reverse Polish Notation) using the Shunting Yard algorithm.
        /// This makes evaluation much simpler and handles operator precedence correctly.
        /// Precedence: ! (highest) > & > | (lowest)
        /// </summary>
        private static List<Token> InfixToPostfix(List<Token> infix)
        {
            var output = new List<Token>();
            var operators = new Stack<Token>();

            foreach (var token in infix)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        output.Add(token);
                        break;

                    case TokenType.Not:
                    case TokenType.And:
                    case TokenType.Or:
                        // Pop operators with higher or equal precedence
                        while (operators.Count > 0 &&
                               operators.Peek().Type != TokenType.LeftParen &&
                               GetPrecedence(operators.Peek().Type) >= GetPrecedence(token.Type))
                        {
                            output.Add(operators.Pop());
                        }
                        operators.Push(token);
                        break;

                    case TokenType.LeftParen:
                        operators.Push(token);
                        break;

                    case TokenType.RightParen:
                        // Pop until we find the matching left paren
                        while (operators.Count > 0 && operators.Peek().Type != TokenType.LeftParen)
                        {
                            output.Add(operators.Pop());
                        }
                        if (operators.Count == 0)
                            throw new ArgumentException("Mismatched parentheses in filter expression");
                        operators.Pop(); // Discard the left paren
                        break;
                }
            }

            // Pop remaining operators
            while (operators.Count > 0)
            {
                var op = operators.Pop();
                if (op.Type == TokenType.LeftParen)
                    throw new ArgumentException("Mismatched parentheses in filter expression");
                output.Add(op);
            }

            return output;
        }

        /// <summary>
        /// Returns operator precedence (higher = evaluated first).
        /// NOT (!) has highest precedence, then AND (&), then OR (|).
        /// </summary>
        private static int GetPrecedence(TokenType type)
        {
            return type switch
            {
                TokenType.Not => 3,  // Highest
                TokenType.And => 2,
                TokenType.Or => 1,   // Lowest
                _ => 0
            };
        }

        /// <summary>
        /// Evaluates a postfix expression using a stack.
        /// </summary>
        private static bool EvaluatePostfix(List<Token> postfix, bool[] filterResults)
        {
            var stack = new Stack<bool>();

            foreach (var token in postfix)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        // 1-based index, convert to 0-based
                        int index = token.Value - 1;
                        if (index < 0 || index >= filterResults.Length)
                            throw new ArgumentException($"Filter rule #{token.Value} does not exist (only {filterResults.Length} filters defined)");
                        stack.Push(filterResults[index]);
                        break;

                    case TokenType.Not:
                        if (stack.Count < 1)
                            throw new ArgumentException("Invalid expression: NOT operator requires one operand");
                        stack.Push(!stack.Pop());
                        break;

                    case TokenType.And:
                        if (stack.Count < 2)
                            throw new ArgumentException("Invalid expression: AND operator requires two operands");
                        var right1 = stack.Pop();
                        var left1 = stack.Pop();
                        stack.Push(left1 && right1);
                        break;

                    case TokenType.Or:
                        if (stack.Count < 2)
                            throw new ArgumentException("Invalid expression: OR operator requires two operands");
                        var right2 = stack.Pop();
                        var left2 = stack.Pop();
                        stack.Push(left2 || right2);
                        break;

                    default:
                        throw new ArgumentException($"Unexpected token type in postfix: {token.Type}");
                }
            }

            if (stack.Count != 1)
                throw new ArgumentException("Invalid expression: malformed filter logic");

            return stack.Pop();
        }

        /// <summary>
        /// Validates a filter logic expression without evaluating it.
        /// Returns true if valid, false otherwise. Populates errorMessage on failure.
        /// </summary>
        public static bool Validate(string expression, int filterCount, out string? errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(expression))
                return true; // Empty is valid

            try
            {
                var tokens = Tokenize(expression);
                var postfix = InfixToPostfix(tokens);

                // Check that all filter references are valid
                foreach (var token in postfix.Where(t => t.Type == TokenType.Number))
                {
                    if (token.Value < 1 || token.Value > filterCount)
                    {
                        errorMessage = $"Filter rule #{token.Value} does not exist (only {filterCount} filters defined)";
                        return false;
                    }
                }

                // Try to evaluate with dummy data to catch structural errors
                var dummyResults = new bool[filterCount];
                EvaluatePostfix(postfix, dummyResults);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Represents a token in the filter logic expression.
        /// </summary>
        private class Token
        {
            public TokenType Type { get; set; }
            public int Value { get; set; } // Only used for Number type

            public override string ToString() => Type == TokenType.Number ? Value.ToString() : Type.ToString();
        }

        /// <summary>
        /// Types of tokens in the filter logic expression.
        /// </summary>
        private enum TokenType
        {
            Number,      // Filter rule number (1, 2, 3, ...)
            And,         // & operator
            Or,          // | operator
            Not,         // ! operator
            LeftParen,   // (
            RightParen   // )
        }
    }
}
