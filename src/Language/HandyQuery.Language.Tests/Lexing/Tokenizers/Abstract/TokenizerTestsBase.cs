using System;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers.Abstract
{
    internal abstract class TokenizerTestsBase
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

        protected void ThenFailed()
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.Token.Should().BeNull();

            testCase.Finished = true;
        }

        public class TestCase
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
            public static TestCase RefreshCurrent(TokenizerTestsBase test)
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