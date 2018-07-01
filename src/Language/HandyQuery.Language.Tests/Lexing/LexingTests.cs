using FluentAssertions;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing
{
    public class LexingTests
    {
        [Test]
        public void Lexer_ShouldBeAbleToTokenizeStatements()
        {
            var config = HandyQueryLanguage.Configure<Person>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName)
                .Build();

            // TODO: implement test query builder?

            var firstName = "FirstName";
            var isEmpty = "is empty";
            var expected = new TokenList()
            {
                new TextLiteralToken(0, firstName.Length, firstName),
                //new WhitespaceToken(9, 1),
                new KeywordToken(10, isEmpty.Length, StatementKeyword.IsEmpty)
            };

            var result = Lexer.Build(config).Tokenize("FirstName is empty");

            result.Tokens.IsSameAs(expected).Should().BeTrue();
        }
        
        // TODO: MOAR tests
    }
}
