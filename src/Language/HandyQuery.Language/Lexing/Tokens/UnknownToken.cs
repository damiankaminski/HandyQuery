using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class UnknownToken : TokenBase
    {
        public override TokenType TokenType => TokenType.Unknown;

        public UnknownToken(int startPosition, int length) : base(startPosition, length)
        {
        }
    }
}