using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class NumberLiteralToken : LiteralTokenBase<object>
    {
        public override TokenType TokenType => TokenType.NumberLiteral;

        public NumberLiteralToken(int startPosition, int length, object value) : base(startPosition, length, value)
        {
        }
    }
}