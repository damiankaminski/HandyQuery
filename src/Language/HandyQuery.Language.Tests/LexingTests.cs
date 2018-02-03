using System.Globalization;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests
{
    public class LexingTests
    {
        private readonly Lexer _lexer;

        public LexingTests()
        {
            var generator = new LexerGenerator();
            _lexer = generator.GenerateLexer();
        }

        [Test]
        public void Lexer_ShouldBeAbleToTokenizeStatements()
        {
            var config = new LanguageConfig<Person>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName)
                .InternalConfig;

            // TODO: implement test query builder

            var firstName = "FirstName";
            var isEmpty = "is empty";
            var expected = new TokenList()
            {
                new ColumnToken(0, firstName.Length, config.GetColumnInfo(firstName)),
                new WhitespaceToken(9, 1),
                //new StatementToken(10, isEmpty.Length, config.GetCultureConfig(CultureInfo.InvariantCulture).GetKeyword(isEmpty))
            };

            var result = _lexer.Tokenize("FirstName is empty", config, CultureInfo.InvariantCulture);

            result.Errors.Should().BeEmpty();
            result.Tokens.IsSameAs(expected).Should().BeTrue();
        }
    }
}
