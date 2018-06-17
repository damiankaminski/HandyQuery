using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Tests.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    internal class KeywordsTokenizerTests : TokenizerTestsBase
    {
        protected override LanguageConfig DefaultConfig => HandyQueryLanguage.Configure<Person>()
            .AddColumn("Name", x => x.FirstName)
            .AddColumn("LastName", x => x.LastName)
            .Build();

        protected override ITokenizer GetTokenizer(LanguageConfig config) => new KeywordTokenizer(config);

        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_all_keywords(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword];
            GivenQuery($"Name |{text}| and");
            WhenTokenized();
            ThenSuccess(keyword);
        }

        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_keywords_defined_at_the_end_of_query(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword];
            GivenQuery($"Name |{text}|");
            WhenTokenized();
            ThenSuccess(keyword);
        }

        [TestCaseSource(nameof(GetAllKeywordsWithWhiteSpaces))]
        public void Should_tokenize_all_keywords_with_multiple_whitespaces(KeywordBase keyword, string text)
        {
            var textWithMultipleWhitespaces = text.Replace(" ", "   \t");
            GivenQuery($"Name |{textWithMultipleWhitespaces}| and");
            WhenTokenized();
            ThenSuccess(keyword);
        }
        
        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_case_insensitively_by_default(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax
                .KeywordsMap[keyword]
                .ToUpper();

            GivenQuery($"Name |{text}| and");
            WhenTokenized();
            ThenSuccess(keyword);
        }

        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_properly_with_case_sensitivity_configured(KeywordBase keyword)
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
                GivenQuery($"Name |{text}| and");
                GivenConfig(config);
                WhenTokenized();
                ThenSuccess(keyword);
            }

            // upper cased - should not be able to tokenize
            {
                var upperCased = config.Syntax.KeywordsMap[keyword].ToUpper();
                GivenQuery($"Name |{upperCased} and");
                GivenConfig(config);
                WhenTokenized();
                ThenFailed();
            }
        }

        [Test]
        public void Should_result_with_error_when_keyword_is_invalid()
        {
            GivenQuery("Name |is or isnt 'Test'");
            WhenTokenized();
            ThenFailed();
        }

        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_result_with_error_when_whitespace_after_token_is_missing(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword].ToUpper();
            if (char.IsLetterOrDigit(text.Last()))
            {
                GivenQuery($"Name |{text}ornot and");
                WhenTokenized();
                ThenFailed();                
            }
            else
            {
                GivenQuery($"Name |{text}|ornot and");
                WhenTokenized();
                ThenSuccess(keyword);
            }
        }

        private static void ThenSuccess(KeywordBase keyword)
        {
            var testCase = TestCase.Current;
            if(testCase.ExpectedLength == null) throw new NullReferenceException();
            
            testCase.Result.Success.Should().BeTrue();
            var token = testCase.Result.Token.As<KeywordToken>();
            token.StartPosition.Should().Be(testCase.Position);
            token.TokenType.Should().Be(TokenType.Keyword);
            token.Keyword.Should().BeEquivalentTo(keyword);
            token.Length.Should().Be(testCase.ExpectedLength);
            
            testCase.Finished = true;
        }
        
        private static IEnumerable<KeywordBase> GetAllKeywords()
            => HandyQueryLanguage.BuildSyntax().Build().KeywordsMap.Keys;
        
        private static IEnumerable<TestCaseData> GetAllKeywordsWithWhiteSpaces()
        {
            return HandyQueryLanguage.BuildSyntax().Build().KeywordsMap
                .Where(x => x.Value.Contains(" "))
                .Select(x => new TestCaseData(x.Key, x.Value));
        }
    }    
}