using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class WhitespaceToken : TokenBase
    {
        public override TokenType TokenType => TokenType.Whitespace;

        public WhitespaceToken(int startPosition, int length) : base(startPosition, length)
        {
        }
    }
}