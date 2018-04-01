using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal class LogicalOperatorToken : KeywordTokenBase
    {
        public override TokenType TokenType => TokenType.LogicalOperator;

        public LogicalOperatorToken(int startPosition, int length, Keyword keyword) 
            : base(startPosition, length, keyword)
        {
        }
    }
}