using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class StatementToken : KeywordTokenBase
    {
        public override TokenType TokenType => TokenType.Statement;

        public StatementToken(int startPosition, int length, Keyword keyword) : base(startPosition, length, keyword)
        {
        }
    }
}