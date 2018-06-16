using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Tests.Lexing.Tokenizers.Abstract;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    internal class TextLiteralTokenizerTests : TokenizerTestsBase
    {
        protected override LanguageConfig DefaultConfig => HandyQueryLanguage.Configure<SearchTrieTests.Person>().Build();
        protected override ITokenizer GetTokenizer(LanguageConfig config) => new TextLiteralTokenizer(config);

        [TestCase("|\"{text}\"")]
        [TestCase("|\"{text}\" and")]
        [TestCase("Name = |\"{text}\" and")]
        [TestCase("Name = |\"{text}\"")]
        public void Should_tokenize_quoted_text(string query)
        {
            const string text = "Foo bar baz";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenSuccess(text, text.Length + 2);
        }
        
        [TestCase("|{text}")]
        [TestCase("|{text}<10")]
        [TestCase("|{text} and")]
        [TestCase("Name = |{text} and")]
        [TestCase("Name = |{text}")]
        public void Should_tokenize_pure_text(string query)
        {
            const string text = "Foo";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenSuccess(text, text.Length);
        }

        [TestCase("|\"{text}")]
        [TestCase("|\"{text} and")]
        [TestCase("Name = |\"{text} and")]
        [TestCase("Name = |\"{text}")]
        public void Should_tokenize_not_closed_text(string query)
        {
            const string text = "Foo";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenPartiallySuccessful(text, text.Length + 1);
        }
        
        [TestCase(@"|'Foo\\'", @"Foo\")]
        [TestCase(@"|'Foo\'bar'", "Foo'bar")]
        [TestCase(@"|'Foo\nbar'", "Foo\nbar")]
        // TODO: all other escape sequences from here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/cf2e3220-dc8d-4de7-96d3-44dd93a52423/what-character-escape-sequences-are-available-in-c?forum=csharpgeneral
        [TestCase(@"|'Foo\bar\baz\foobar'", "Foobarbazfoobar")] // invalid escape sequences are ignored
        public void Should_tokenize_with_espace_sequences(string query, string expectedtext)
        {
            query = query.Replace("'", "\"");
            expectedtext = expectedtext.Replace("'", "\"");
            GivenQuery(query);
            WhenTokenized();
            ThenSuccess(expectedtext, expectedtext.Length + 2);
        }
        
        // TODO: new line
        
        private static void ThenSuccess(string expectedText, int? expectedLength = null)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeTrue();
            testCase.Result.IsPartiallySuccessful.Should().BeFalse();
            var token = testCase.Result.Token.As<TextLiteralToken>();
            token.StartPosition.Should().Be(testCase.Position);
            token.TokenType.Should().Be(TokenType.TextLiteral);
            token.Length.Should().Be(expectedLength ?? expectedText.Length);
            token.Value.Should().Be(expectedText);
            
            testCase.Finished = true;
        }
        
        private static void ThenPartiallySuccessful(string expectedText, int? expectedLength = null)
        {
            var testCase = TestCase.Current;
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.IsPartiallySuccessful.Should().BeTrue();
            var token = testCase.Result.Token.As<TextLiteralToken>();
            token.StartPosition.Should().Be(testCase.Position);
            token.TokenType.Should().Be(TokenType.TextLiteral);
            token.Length.Should().Be(expectedLength ?? expectedText.Length);
            token.Value.Should().Be(expectedText);
            
            testCase.Finished = true;
        }
    }
}