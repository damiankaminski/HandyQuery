using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    internal sealed class SyntaxConfig
    {
        public readonly bool KeywordCaseSensitive;
        public readonly bool ColumnNameCaseSensitive;

        public readonly char StringLiteralIdentifier = '"';
        public readonly char NumberDecimalSeparator = '.';
        public readonly IEnumerable<string> DateTimeFormats = new[] {"M/d/yyyy H:m", "M/d/yyyy"};

        public readonly CultureInfo CultureInfo;
        
        public readonly IReadOnlyDictionary<KeywordBase, string> KeywordsMap = new Dictionary<KeywordBase, string>()
        {
            {CompareOperatorKeyword.Equal, "="},
            {CompareOperatorKeyword.NotEqual, "!="},
            {CompareOperatorKeyword.GreaterThan, ">"},
            {CompareOperatorKeyword.LessThan, "<"},
            {CompareOperatorKeyword.GreaterThanOrEqual, ">="},
            {CompareOperatorKeyword.LessThanOrEqual, "<="},
            {CompareOperatorKeyword.StartsWith, "starts with"},
            {CompareOperatorKeyword.EndsWith, "ends with"},
            {CompareOperatorKeyword.Contains, "contains"},
            {StatementKeyword.IsEmpty, "is empty"},
            {StatementKeyword.IsNotEmpty, "is not empty"},
            {StatementKeyword.IsTrue, "is true"},
            {StatementKeyword.IsFalse, "is false"},
            {LogicalOperatorKeyword.And, "and"},
            {LogicalOperatorKeyword.Or, "or"},
            {TrivialKeyword.ParenOpen, "("},
            {TrivialKeyword.ParenClose, ")"},
            {TrivialKeyword.ParamsSeparator, ","},
            {TrivialKeyword.Null, "null"}
        };

        public readonly IReadOnlyCollection<char> ReservedChars;

        public SyntaxConfig(bool keywordCaseSensitive, bool columnNameCaseSensitive)
        {
            KeywordCaseSensitive = keywordCaseSensitive;
            ColumnNameCaseSensitive = columnNameCaseSensitive;

            ReservedChars = FindReservedChars();

            var cultureInfo = new CultureInfo(CultureInfo.InvariantCulture.LCID)
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = NumberDecimalSeparator.ToString()
                    // TODO: other formatting stuff could be needed?
                }
            };
            CultureInfo = CultureInfo.ReadOnly(cultureInfo);
        }

        // TODO: change to [azAZ0-9]; otherwise new operators will break compatibility
        private char[] FindReservedChars()
        {
            var fixedReservedChars = new[]
            {
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