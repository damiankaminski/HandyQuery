using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    internal sealed class SyntaxConfig
    {
        public readonly bool KeywordCaseSensitive;
        public readonly bool ColumnNameCaseSensitive;

        public readonly char ParenOpen = '(';
        public readonly char ParenClose = ')';

        public readonly char ParamsOpen = '(';
        public readonly char ParamsClose = ')';

        public readonly char StringLiteralIdentifier = '"';

        public readonly IEnumerable<string> DateTimeFormats = new[] {"M/d/yyyy H:m", "M/d/yyyy"};
        public readonly string NullConstant = "null"; // TODO: move to keywords?
        public readonly char NumberDecimalSeparator = '.';

        public readonly IReadOnlyDictionary<Keyword, string> KeywordsMap = new Dictionary<Keyword, string>()
        {
            {CompareOperatorKeyword.Equal, "="},
            {CompareOperatorKeyword.NotEqual, "!="},
            {CompareOperatorKeyword.GreaterThan, ">"},
            {CompareOperatorKeyword.LessThan, "<"},
            {CompareOperatorKeyword.GreaterThanOrEqual, ">="},
            {CompareOperatorKeyword.LessThanOrEqual, ">="},
            {CompareOperatorKeyword.StartsWith, "starts with"},
            {CompareOperatorKeyword.EndsWith, "ends with"},
            {CompareOperatorKeyword.Contains, "contains"},
            {StatementKeyword.Empty, "is empty"},
            {StatementKeyword.NotEmpty, "is not empty"},
            {StatementKeyword.IsTrue, "is true"},
            {StatementKeyword.IsFalse, "is false"},
            {LogicalOperatorKeyword.And, "and"},
            {LogicalOperatorKeyword.Or, "or"}
        };

        public readonly IReadOnlyCollection<CompareOperatorKeyword> CompareOperators = new[]
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

        public readonly IReadOnlyCollection<StatementKeyword> Statements = new[]
        {
            StatementKeyword.Empty,
            StatementKeyword.NotEmpty,
            StatementKeyword.IsTrue,
            StatementKeyword.IsFalse
        };

        public readonly IReadOnlyCollection<LogicalOperatorKeyword> LogicalOperators = new[]
        {
            LogicalOperatorKeyword.And,
            LogicalOperatorKeyword.Or
        };

        public readonly IReadOnlyCollection<char> ReservedChars;

        public SyntaxConfig(bool keywordCaseSensitive, bool columnNameCaseSensitive)
        {
            KeywordCaseSensitive = keywordCaseSensitive;
            ColumnNameCaseSensitive = columnNameCaseSensitive;

            ReservedChars = FindReservedChars();
        }

        // TODO: change to [azAZ0-9]; otherwise new operators will break compatibility
        private char[] FindReservedChars()
        {
            var fixedReservedChars = new[]
            {
                ParamsClose,
                ParamsOpen,
                ParenClose,
                ParenOpen,
                StringLiteralIdentifier
            };

            var calculatedReservedChars = KeywordsMap
                .Values
                .Where(x => char.IsLetterOrDigit(x[0]) == false)
                .Select(x => x[0])
                .WithoutDuplicates()
                .ToArray();

            var reservedChars = new char[calculatedReservedChars.Length + fixedReservedChars.Length];

            for (int index = 0; index < calculatedReservedChars.Length; index++)
            {
                reservedChars[index] = calculatedReservedChars[index];
            }

            for (int i = 0; i < fixedReservedChars.Length; i++)
            {
                reservedChars[calculatedReservedChars.Length + i] = fixedReservedChars[i];
            }

            return reservedChars;
        }
    }
}