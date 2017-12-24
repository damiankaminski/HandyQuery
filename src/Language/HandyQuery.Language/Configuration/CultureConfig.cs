using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Configuration
{
    public sealed class CultureConfig : ICultureConfig
    {
        internal CultureInfo Culture { get; }

        public IEnumerable<string> DateTimeFormats { get; set; } = new[] { "M/d/yyyy H:m", "M/d/yyyy" };
        public string NullConstant { get; set; } = "null";
        public char NumberDecimalSeparator { get; set; } = '.';

        private readonly Dictionary<Keyword, string> _underlyingKeywords;
        IReadOnlyDictionary<Keyword, string> ICultureConfig.Keywords => _underlyingKeywords;

        public CultureConfig(CultureInfo culture)
        {
            Culture = culture;

            // TODO: expose public API to change keywords translations (use enums to avoid public Keyword class?)
            // TODO: allow to change keyword translation on per column basis (.Column(x => x.Name, c => c.Adjust(Statement.Empty, "empty")))
            _underlyingKeywords = new Dictionary<Keyword, string>()
            {
                { CompareOperatorKeyword.Equal, "=" },
                { CompareOperatorKeyword.NotEqual, "!=" },
                { CompareOperatorKeyword.GreaterThan, ">" },
                { CompareOperatorKeyword.LessThan, "<" },
                { CompareOperatorKeyword.GreaterThanOrEqual, ">=" },
                { CompareOperatorKeyword.LessThanOrEqual, ">=" },
                { CompareOperatorKeyword.StartsWith, "starts with" },
                { CompareOperatorKeyword.EndsWith, "ends with" },
                { CompareOperatorKeyword.Contains, "contains" },
                { StatementKeyword.Empty, "is empty" },
                { StatementKeyword.NotEmpty, "is not empty" },
                { StatementKeyword.IsTrue, "is true" },
                { StatementKeyword.IsFalse, "is false" },
                { LogicalOperatorKeyword.And, "and" },
                { LogicalOperatorKeyword.Or, "or" }
            };
        }
    }

    internal static class CultureConfigExtensions
    {
        public static string GetKeywordText(this ICultureConfig config, Keyword keyword)
        {
            string result;
            if (config.Keywords.TryGetValue(keyword, out result) == false)
            {
                throw new QueryLanguageException("Missing keyword translations.");
            }

            return result;
        }

        public static Keyword GetKeyword(this ICultureConfig config, string keywordText)
        {
            // TODO: two way dictionary
            var keyword = config.Keywords.Where(x => x.Value == keywordText).Select(x => x.Key).FirstOrDefault();
            return keyword;
        }
    }
}