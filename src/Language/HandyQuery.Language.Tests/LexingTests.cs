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
        private readonly LexerGenerator _lexerGenerator;

        public LexingTests()
        {
            _lexerGenerator = new LexerGenerator();
        }

        //[Test]
        public void Lexer_ShouldBeAbleToTokenizeStatements()
        {
            var config = HandyQueryLanguage.Configure<Person>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName)
                .Build();

            // TODO: implement test query builder

            var firstName = "FirstName";
            //var isEmpty = "is empty";
            var expected = new TokenList()
            {
                new ColumnToken(0, firstName.Length, config.GetColumnInfo(firstName)),
                new WhitespaceToken(9, 1),
                //new StatementToken(10, isEmpty.Length, config.GetCultureConfig(CultureInfo.InvariantCulture).GetKeyword(isEmpty))
            };

            var result = _lexerGenerator.GenerateLexer(config).Tokenize("FirstName is empty");

            result.Errors.Should().BeEmpty();
            result.Tokens.IsSameAs(expected).Should().BeTrue();
        }
    }
}
