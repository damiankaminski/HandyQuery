using HandyQuery.Language.Lexing;

namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class LogicalOperatorKeyword : Keyword
    {
        public static readonly LogicalOperatorKeyword And = new LogicalOperatorKeyword(LogicalOperator.And);
        public static readonly LogicalOperatorKeyword Or = new LogicalOperatorKeyword(LogicalOperator.Or);

        public LogicalOperator Type { get; }
        public override TokenType TokenType { get; } = TokenType.LogicalOperator;

        private LogicalOperatorKeyword(LogicalOperator type)
        {
            Type = type;
        }
    }

    internal enum LogicalOperator
    {
        And,
        Or
    }
}