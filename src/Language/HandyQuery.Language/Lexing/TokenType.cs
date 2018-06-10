namespace HandyQuery.Language.Lexing
{
    internal enum TokenType
    {
        Unknown,
        Whitespace,
        TextLiteral,
        DateLiteral,
        NumberLiteral,
        Keyword
    }
}