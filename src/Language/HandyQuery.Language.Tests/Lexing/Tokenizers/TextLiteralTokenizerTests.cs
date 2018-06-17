using System;
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

        [TestCase("|\"{text}\"|")]
        [TestCase("|\"{text}\"| and")]
        [TestCase("Name = |\"{text}\"| and")]
        [TestCase("Name = |\"{text}\"|")]
        public void Should_tokenize_quoted_text(string query)
        {
            const string text = "Foo bar baz";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenSuccess(text);
        }
        
        [TestCase("|{text}|")]
        [TestCase("|{text}|<10")]
        [TestCase("|{text}| and")]
        [TestCase("Name = |{text}| and")]
        [TestCase("Name = |{text}|")]
        public void Should_tokenize_pure_text(string query)
        {
            const string text = "Foo";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenSuccess(text);
        }

        [TestCase("|\"{text}|")]
        [TestCase("|\"{text}| and")]
        [TestCase("Name = |\"{text}| and")]
        [TestCase("Name = |\"{text}|")]
        [TestCase("Name = |\"{text}|\ntest\"")]
        [TestCase("Name = |\"{text}|\ntest\" and")]
        public void Should_tokenize_not_closed_text(string query)
        {
            const string text = "Foo";
            GivenQuery(query.Replace("{text}", text));
            WhenTokenized();
            ThenPartiallySuccessful(text);
        }
        
        [TestCase(@"|'Foo\\'|", @"Foo\")]
        [TestCase(@"|'Foo\\'|test", @"Foo\")]
        [TestCase(@"|'Foo\'bar'|", "Foo'bar")]
        [TestCase(@"|'Foo\0bar'|", "Foo\0bar")]
        [TestCase(@"|'Foo\abar'|", "Foo\abar")]
        [TestCase(@"|'Foo\bbar'|", "Foo\bbar")]
        [TestCase(@"|'Foo\fbar'|", "Foo\fbar")]
        [TestCase(@"|'Foo\nbar'|", "Foo\nbar")]
        [TestCase(@"|'Foo\rbar'|", "Foo\rbar")]
        [TestCase(@"|'Foo\tbar'|", "Foo\tbar")]
        [TestCase(@"|'Foo\vbar'|", "Foo\vbar")]
        [TestCase(@"|'Foo\Bar\Baz\Foobar'|", "FooBarBazFoobar")] // invalid escape sequences are ignored
        public void Should_tokenize_with_espace_sequences(string query, string expectedtext)
        {
            query = query.Replace("'", "\"");
            expectedtext = expectedtext.Replace("'", "\"");
            GivenQuery(query);
            WhenTokenized();
            ThenSuccess(expectedtext);
        }
        
        private static void ThenSuccess(string expectedTokenValue)
        {
            var testCase = TestCase.Current;
            if(testCase.ExpectedLength == null) throw new NullReferenceException();
            
            testCase.Result.Success.Should().BeTrue();
            testCase.Result.IsPartiallySuccessful.Should().BeFalse();
            var token = testCase.Result.Token.As<TextLiteralToken>();
            token.StartPosition.Should().Be(testCase.Position);
            token.TokenType.Should().Be(TokenType.TextLiteral);
            token.Length.Should().Be(testCase.ExpectedLength);
            token.Value.Should().Be(expectedTokenValue);
            
            testCase.Finished = true;
        }
        
        private static void ThenPartiallySuccessful(string expectedText)
        {
            var testCase = TestCase.Current;
            if(testCase.ExpectedLength == null) throw new NullReferenceException();
            
            testCase.Result.Success.Should().BeFalse();
            testCase.Result.IsPartiallySuccessful.Should().BeTrue();
            var token = testCase.Result.Token.As<TextLiteralToken>();
            token.StartPosition.Should().Be(testCase.Position);
            token.TokenType.Should().Be(TokenType.TextLiteral);
            token.Length.Should().Be(testCase.ExpectedLength);
            token.Value.Should().Be(expectedText);
            
            testCase.Finished = true;
        }
    }
}