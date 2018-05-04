using System;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    internal abstract class KeywordTokenizerTestsBase
    {
        protected abstract LanguageConfig DefaultConfig { get; }
        
        protected abstract TokenType ExpectedTokenType { get; }

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
            var lexerRuntimeInfo = new LexerRuntimeInfo(new LexerStringReader(testCase.Query, testCase.Position), testCase.Config);
            testCase.Result = GetTokenizer(testCase.Config).Tokenize(ref lexerRuntimeInfo);
        }

        protected void ThenSuccess(Keyword keyword, string expectedText)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeTrue();
            testCase.Result.Token.TokenType.Should().Be(ExpectedTokenType);
            var token = testCase.Result.Token.As<KeywordTokenBase>();
            token.StartPosition.Should().Be(5);
            token.TokenType.Should().Be(keyword.TokenType);
            token.Keyword.Should().BeEquivalentTo(keyword);
            token.Length.Should().Be(expectedText.Length);
            
            testCase.Finished = true;
        }

        protected void ThenFailedWithError(ErrorId expectedErrorId)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.Token.Should().BeNull();
            testCase.Result.Error.Id.Should().Be(expectedErrorId);

            testCase.Finished = true;
        }
        
        protected void ThenFailedWithRange(int position, int length)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.Token.Should().BeNull();
            testCase.Result.Error.Range.Position.Should().Be(position);
            testCase.Result.Error.Range.Length.Should().Be(length);

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