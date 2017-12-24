using System;
using System.Collections.Generic;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Configuration
{
    internal sealed class SyntaxInfo
    {
        public SyntaxInfo(ISyntaxConfig config)
        {
            Config = config;

            _compareOperators = new []
            {
                CompareOperatorKeyword.Equal,
                CompareOperatorKeyword.NotEqual,
                CompareOperatorKeyword.GreaterThan,
                CompareOperatorKeyword.LessThan,
                CompareOperatorKeyword.GreaterThanOrEqual,
                CompareOperatorKeyword.LessThanOrEqual,
                CompareOperatorKeyword.StartsWith,
                CompareOperatorKeyword.EndsWith,
                CompareOperatorKeyword.Contains
            };

            _statements = new []
            {
                StatementKeyword.Empty,
                StatementKeyword.NotEmpty,
                StatementKeyword.IsTrue,
                StatementKeyword.IsFalse
            };

            _logicalOperators = new []
            {
                LogicalOperatorKeyword.And, 
                LogicalOperatorKeyword.Or 
            };

            RecalculateReservedChars();
        }

        public ISyntaxConfig Config { get; }

        private readonly CompareOperatorKeyword[] _compareOperators;
        private readonly StatementKeyword[] _statements;
        private readonly LogicalOperatorKeyword[] _logicalOperators;
        private char[] _reservedChars;

        public IReadOnlyCollection<CompareOperatorKeyword> CompareOperators => _compareOperators;
        public IReadOnlyCollection<StatementKeyword> Statements => _statements;
        public IReadOnlyCollection<LogicalOperatorKeyword> LogicalOperators => _logicalOperators;
        public IReadOnlyCollection<char> ReservedChars => _reservedChars;

        internal void RecalculateReservedChars()
        {
            var fixedReservedChars = new[]
            {
                Config.ParamsClose,
                Config.ParamsOpen,
                Config.ParenClose,
                Config.ParenOpen,
                Config.StringLiteralIdentifier
            };

            var chars = fixedReservedChars;

            // TODO: each culture can define its own syntax for keywords thus reserved chars should take all of them into account
            // code below is for a single culture, it needs to be changed

            //            var fromKeywords = config.Keywords
            //                .Where(x => char.IsLetterOrDigit(x.Syntax[0]) == false)
            //                .Select(x => x.Syntax[0])
            //                .ToArray();
            //
            //            var chars = new char[fromKeywords.Length + 3];
            //
            //            for (int index = 0; index < fromKeywords.Length; index++)
            //            {
            //                chars[index] = fromKeywords[index];
            //            }
            //
            //            chars[fromKeywords.Length] = config.ParenOpen;
            //            chars[fromKeywords.Length + 1] = config.ParenClose;
            //            chars[fromKeywords.Length + 2] = config.StringLiteralIdentifier;

            _reservedChars = chars;
        }
    }
}