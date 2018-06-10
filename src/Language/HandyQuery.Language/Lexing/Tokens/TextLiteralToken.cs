using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class TextLiteralToken : LiteralTokenBase<string>
    {
        public override TokenType TokenType => TokenType.TextLiteral;

        public TextLiteralToken(int startPosition, int length, string value) : base(startPosition, length, value)
        {
        }
    }
}