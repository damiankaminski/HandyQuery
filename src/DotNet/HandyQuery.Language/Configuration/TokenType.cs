namespace HandyQuery.Language.Configuration
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