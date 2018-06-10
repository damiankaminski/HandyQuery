namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class LogicalOperatorKeyword : Keyword
    {
        public static readonly LogicalOperatorKeyword And = new LogicalOperatorKeyword(LogicalOperatorType.And);
        public static readonly LogicalOperatorKeyword Or = new LogicalOperatorKeyword(LogicalOperatorType.Or);

        public LogicalOperatorType LogicalOperatorType { get; }
        public override KeywordType KeywordType { get; } = KeywordType.LogicalOperator;

        private LogicalOperatorKeyword(LogicalOperatorType logicalOperatorType)
        {
            LogicalOperatorType = logicalOperatorType;
        }
        
        public override string ToString()
        {
            return $"{KeywordType}: {LogicalOperatorType.ToString()}";
        }
    }

    internal enum LogicalOperatorType
    {
        And,
        Or
    }
}