namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class CompareOperatorKeyword : Keyword
    {
        public static readonly CompareOperatorKeyword Equal = new CompareOperatorKeyword(CompareOperatorType.Equal);
        public static readonly CompareOperatorKeyword NotEqual = new CompareOperatorKeyword(CompareOperatorType.NotEqual);
        public static readonly CompareOperatorKeyword GreaterThan = new CompareOperatorKeyword(CompareOperatorType.GreaterThan);
        public static readonly CompareOperatorKeyword LessThan = new CompareOperatorKeyword(CompareOperatorType.LessThan);
        public static readonly CompareOperatorKeyword GreaterThanOrEqual = new CompareOperatorKeyword(CompareOperatorType.GreaterThanOrEqual);
        public static readonly CompareOperatorKeyword LessThanOrEqual = new CompareOperatorKeyword(CompareOperatorType.LessThanOrEqual);
        public static readonly CompareOperatorKeyword StartsWith = new CompareOperatorKeyword(CompareOperatorType.StartsWith);
        public static readonly CompareOperatorKeyword EndsWith = new CompareOperatorKeyword(CompareOperatorType.EndsWith);
        public static readonly CompareOperatorKeyword Contains = new CompareOperatorKeyword(CompareOperatorType.Contains);

        public CompareOperatorType CompareOperatorType { get; }
        public override KeywordType KeywordType { get; } = KeywordType.CompareOperator;

        private CompareOperatorKeyword(CompareOperatorType compareOperatorType)
        {
            CompareOperatorType = compareOperatorType;
        }
        
        public override string ToString()
        {
            return $"{KeywordType}: {CompareOperatorType.ToString()}";
        }
    }

    internal enum CompareOperatorType
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        StartsWith,
        EndsWith,
        Contains
    }
}