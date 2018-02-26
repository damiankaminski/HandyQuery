namespace HandyQuery.Language.Lexing
{
    internal enum TokenType
    {
        Unknown,
        Whitespace,
        Column,
        CompareOperator,
        LogicalOperator,
        Literal,
        Function,
        Statement,
        ParenOpen,
        ParenClose
    }
}