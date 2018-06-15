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
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    internal class KeywordsTokenizerTests : KeywordTokenizerTestsBase
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
            GivenQuery($"Name |{text} and");
            WhenTokenized();
            ThenSuccess(keyword, text);
        }

        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_keywords_defined_at_the_end_of_query(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax.KeywordsMap[keyword];
            GivenQuery($"Name |{text}");
            WhenTokenized();
            ThenSuccess(keyword, text);
        }

        [TestCaseSource(nameof(GetAllKeywordsWithWhiteSpaces))]
        public void Should_tokenize_all_keywords_with_multiple_whitespaces(KeywordBase keyword, string text)
        {
            var textWithMultipleWhitespaces = text.Replace(" ", "   \t");
            GivenQuery($"Name |{textWithMultipleWhitespaces} and");
            WhenTokenized();
            ThenSuccess(keyword, textWithMultipleWhitespaces);
        }
        
        [TestCaseSource(nameof(GetAllKeywords))]
        public void Should_tokenize_case_insensitively_by_default(KeywordBase keyword)
        {
            var text = DefaultConfig.Syntax
                .KeywordsMap[keyword]
                .ToUpper();

            GivenQuery($"Name |{text} and");
            WhenTokenized();
            ThenSuccess(keyword, text);
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
                GivenQuery($"Name |{text}ornot and");
                WhenTokenized();
                ThenSuccess(keyword, text);
            }
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
    
    internal abstract class KeywordTokenizerTestsBase
    {
        protected abstract LanguageConfig DefaultConfig { get; }

        protected abstract ITokenizer GetTokenizer(LanguageConfig config);
        
        protected void GivenQuery(string query)
        {
            var caretIndex = query.IndexOf("|", StringComparison.Ordinal);
            var withoutCaret = $"{query.Substring(0, caretIndex)}{query.Substring(caretIndex + 1)}";

            var testCase = TestCase.RefreshCurrent(this);
            testCase.Query = withoutCaret;
            testCase.Position = caretIndex;
        }

        protected void GivenConfig(LanguageConfig config)
        {
            var testCase = TestCase.RefreshCurrent(this);
            testCase.Config = config;
        }

        protected void WhenTokenized()
        {
            var testCase = TestCase.Current;
            var lexerRuntimeInfo = new LexerRuntimeInfo(new LexerStringReader(testCase.Query, testCase.Position));
            testCase.Result = GetTokenizer(testCase.Config).Tokenize(ref lexerRuntimeInfo);
        }

        protected void ThenSuccess(KeywordBase keyword, string expectedText)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeTrue();
            var token = testCase.Result.Token.As<KeywordToken>();
            token.StartPosition.Should().Be(5);
            token.TokenType.Should().Be(TokenType.Keyword);
            token.Keyword.Should().BeEquivalentTo(keyword);
            token.Length.Should().Be(expectedText.Length);
            
            testCase.Finished = true;
        }

        protected void ThenFailed()
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.Token.Should().BeNull();

            testCase.Finished = true;
        }
        
        private class TestCase
        {
            public string Query { get; set; }
            public int Position { get; set; }
            public LanguageConfig Config { get; set; }

            public TokenizationResult Result { get; set; }

            public bool Finished { private get; set; }

            public static TestCase Current { get; private set; }

            private TestCase(LanguageConfig defaultConfig)
            {
                Config = defaultConfig;
            }

            /// <summary>
            /// Creates new instance of test case if current one has been aleady used through
            /// full pipeline (i.e. any of `Then...` method was called at least once with current test case).
            /// </summary>
            /// <returns></returns>
            public static TestCase RefreshCurrent(KeywordTokenizerTestsBase test)
            {
                if (Current == null)
                {
                    Current = new TestCase(test.DefaultConfig);
                    return Current;
                }

                Current = Current.Finished ? new TestCase(test.DefaultConfig) : Current;
                return Current;
            }
        }
    }
}