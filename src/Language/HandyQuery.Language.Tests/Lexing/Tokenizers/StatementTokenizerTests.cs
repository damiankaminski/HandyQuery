using System;
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
    public class StatementTokenizerTests
    {
        private static readonly LanguageConfig DefaultConfig = HandyQueryLanguage.Configure<Person>()
            .AddColumn("Name", x => x.FirstName)
            .AddColumn("LastName", x => x.LastName)
            .Build();

        private static ITokenizer Tokenizer => new StatementTokenizer();

        [Test]
        public void Should_tokenize_all_statements()
        {
            foreach (var statementKeyword in DefaultConfig.Syntax.Statements)
            {
                var statementText = DefaultConfig.Syntax.KeywordsMap[statementKeyword];
                GivenQuery($"Name |{statementText} and");
                WhenTokenized();
                ThenSuccess(statementKeyword, statementText);
            }
        }
        
        [Test]
        public void Should_tokenize_statements_defined_at_the_end_of_query()
        {
            foreach (var statementKeyword in DefaultConfig.Syntax.Statements)
            {
                var statementText = DefaultConfig.Syntax.KeywordsMap[statementKeyword];
                GivenQuery($"Name |{statementText}");
                WhenTokenized();
                ThenSuccess(statementKeyword, statementText);
            }
        }

        [Test]
        public void Should_tokenize_case_insensitively_by_default()
        {
            foreach (var statementKeyword in DefaultConfig.Syntax.Statements)
            {
                var statementText = DefaultConfig.Syntax
                    .KeywordsMap[statementKeyword]
                    .ToUpper();

                GivenQuery($"Name |{statementText} and");
                WhenTokenized();
                ThenSuccess(statementKeyword, statementText);
            }
        }

        [Test]
        public void Should_tokenize_properly_with_case_sensitivity_configured()
        {
            var syntax = HandyQueryLanguage.BuildSyntax().WithCaseSensitiveKeywords();
            var config = HandyQueryLanguage.Configure<Person>(syntax).Build();

            // original case - should be able to tokenize
            foreach (var statementKeyword in config.Syntax.Statements)
            {
                var statementText = config.Syntax
                    .KeywordsMap[statementKeyword];

                GivenQuery($"Name |{statementText} and");
                GivenConfig(config);
                WhenTokenized();
                ThenSuccess(statementKeyword, statementText);
            }

            // upper cased - should not be able to tokenize
            foreach (var statementKeyword in config.Syntax.Statements)
            {
                var statementText = config.Syntax
                    .KeywordsMap[statementKeyword]
                    .ToUpper();

                GivenQuery($"Name |{statementText} and");
                GivenConfig(config);
                WhenTokenized();
                ThenFailedWithError(ErrorId.StatementNotFound);
            }
        }

        [Test]
        public void Should_result_with_error_when_statement_is_invalid()
        {
            GivenQuery("Name |is or isnt 'Test'");
            WhenTokenized();
            ThenFailedWithError(ErrorId.StatementNotFound);
        }

        [Test]
        public void Should_result_with_error_when_whitespace_after_token_is_missing()
        {
            GivenQuery("Name |is emptyornot and");
            WhenTokenized();
            ThenFailedWithError(ErrorId.StatementNotFound);
        }
        
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private class TestCase
        {
            public string Query { get; set; }
            public int Position { get; set; }
            public LanguageConfig Config { get; set; }

            public TokenizationResult Result { get; set; }

            public bool Finished { private get; set; }

            public static TestCase Current { get; private set; }

            private TestCase()
            {
                Config = DefaultConfig;
            }

            /// <summary>
            /// Creates new instance of test case if current one has been aleady used through
            /// full pipeline (i.e. any of `Then...` method was called at least once with current test case).
            /// </summary>
            /// <returns></returns>
            public static TestCase RefreshCurrent()
            {
                if (Current == null)
                {
                    Current = new TestCase();
                    return Current;
                }

                Current = Current.Finished ? new TestCase() : Current;
                return Current;
            }
        }

        private static void GivenQuery(string query)
        {
            var caretIndex = query.IndexOf("|", StringComparison.Ordinal);
            var withoutCaret = $"{query.Substring(0, caretIndex)}{query.Substring(caretIndex + 1)}";

            var testCase = TestCase.RefreshCurrent();
            testCase.Query = withoutCaret;
            testCase.Position = caretIndex;
        }

        private static void GivenConfig(LanguageConfig config)
        {
            var testCase = TestCase.RefreshCurrent();
            testCase.Config = config;
        }

        private static void WhenTokenized()
        {
            var testCase = TestCase.Current;
            var lexerRuntimeInfo = new LexerRuntimeInfo(new LexerStringReader(testCase.Query, testCase.Position), testCase.Config);
            testCase.Result = Tokenizer.Tokenize(ref lexerRuntimeInfo);
        }

        private static void ThenSuccess(StatementKeyword statementKeyword, string expectedStatementText)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeTrue();
            testCase.Result.Token.TokenType.Should().Be(TokenType.Statement);
            var token = testCase.Result.Token.As<StatementToken>();
            token.StartPosition.Should().Be(5);
            token.TokenType.Should().Be(statementKeyword.TokenType);
            token.Keyword.Should().BeEquivalentTo(statementKeyword);
            token.Length.Should().Be(expectedStatementText.Length);

            testCase.Finished = true;
        }

        private static void ThenFailedWithError(ErrorId expectedErrorId)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.Token.Should().BeNull();
            testCase.Result.Error.Id.Should().Be(expectedErrorId);

            testCase.Finished = true;
        }
    }
}