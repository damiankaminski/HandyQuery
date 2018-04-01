using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal class CompareOperatorToken : KeywordTokenBase
    {
        public override TokenType TokenType => TokenType.CompareOperator;

        public CompareOperatorToken(int startPosition, int length, Keyword keyword) 
            : base(startPosition, length, keyword)
        {
        }
    }
}