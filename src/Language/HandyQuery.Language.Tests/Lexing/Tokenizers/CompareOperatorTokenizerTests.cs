using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    internal class CompareOperatorTokenizerTests : KeywordTokenizerTestsBase
    {
        protected override LanguageConfig DefaultConfig => HandyQueryLanguage.Configure<Person>()
            .AddColumn("Name", x => x.FirstName)
            .AddColumn("LastName", x => x.LastName)
            .Build();

        protected override ITokenizer GetTokenizer(LanguageConfig config) => new CompareOperatorTokenizer(config);

        protected override TokenType ExpectedTokenType => TokenType.CompareOperator;

        [TestCaseSource(nameof(GetAllCompareOperators))]
        public void Should_tokenize_all_compare_operators(Keyword keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword];
            GivenQuery($"Name |{text} and");
            WhenTokenized();
            ThenSuccess(keyword, text);
        }

        [TestCaseSource(nameof(GetAllCompareOperators))]
        public void Should_tokenize_compare_operators_defined_at_the_end_of_query(Keyword keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword];
            GivenQuery($"Name |{text}");
            WhenTokenized();
            ThenSuccess(keyword, text);
        }

        [TestCaseSource(nameof(GetAllCompareOperators))]
        public void Should_tokenize_case_insensitively_by_default(Keyword keyword)
        {
            var text = DefaultConfig.Syntax
                .KeywordsMap[keyword]
                .ToUpper();

            GivenQuery($"Name |{text} and");
            WhenTokenized();
            ThenSuccess(keyword, text);
        }

        [TestCaseSource(nameof(GetAllCompareOperators))]
        public void Should_tokenize_properly_with_case_sensitivity_configured(Keyword keyword)
        {
            var syntax = HandyQueryLanguage.BuildSyntax().WithCaseSensitiveKeywords();
            var config = HandyQueryLanguage.Configure<Person>(syntax).Build();
            var text = config.Syntax.KeywordsMap[keyword];

            if (text.All(x => !char.IsLetterOrDigit(x)))
            {
                // special characters could be case invariant
                return;
            }

            // original case - should be able to tokenize
            {
                GivenQuery($"Name |{text} and");
                GivenConfig(config);
                WhenTokenized();
                ThenSuccess(keyword, text);
            }

            // upper cased - should not be able to tokenize
            {
                var upperCased = config.Syntax.KeywordsMap[keyword].ToUpper();
                GivenQuery($"Name |{upperCased} and");
                GivenConfig(config);
                WhenTokenized();
                ThenFailedWithError(ErrorId.CompareOperatorNotFound);
            }
        }

        [Test]
        public void Should_result_with_error_when_compare_operator_is_invalid()
        {
            GivenQuery("Name |is or isnt 'Test'");
            WhenTokenized();
            ThenFailedWithError(ErrorId.CompareOperatorNotFound);
        }
        
        [TestCase("Name |is or isnt 'Test'", 5, 2)]
        [TestCase("Name |isor isnt 'Test'", 5, 4)]
        [TestCase("Name \nthat |is or isnt 'Test'", 11, 2)]
        public void Should_set_correct_error_position(string query, int position, int length)
        {
            GivenQuery(query);
            WhenTokenized();
            ThenFailedWithRange(position, length);
        }

        [TestCaseSource(nameof(GetAllCompareOperators))]
        public void Should_result_with_error_when_whitespace_after_token_is_missing(Keyword keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword].ToUpper();
            if (char.IsLetterOrDigit(text.Last()))
            {
                GivenQuery($"Name |{text}ornot and");
                WhenTokenized();
                ThenFailedWithError(ErrorId.CompareOperatorNotFound);                
            }
            else
            {
                GivenQuery($"Name |{text}ornot and");
                WhenTokenized();
                ThenSuccess(keyword, text);
            }
        }

        private static IEnumerable<Keyword> GetAllCompareOperators()
            => HandyQueryLanguage.BuildSyntax().Build().CompareOperators;
    }
}