using System;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class DateLiteralToken : LiteralTokenBase<DateTime>
    {
        public override TokenType TokenType => TokenType.DateLiteral;

        public DateLiteralToken(int startPosition, int length, DateTime value) : base(startPosition, length, value)
        {
        }
    }
}