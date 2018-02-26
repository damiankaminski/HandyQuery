using HandyQuery.Language.Lexing;

namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class CompareOperatorKeyword : Keyword
    {
        public static readonly CompareOperatorKeyword Equal = new CompareOperatorKeyword(CompareOperator.Equal);
        public static readonly CompareOperatorKeyword NotEqual = new CompareOperatorKeyword(CompareOperator.NotEqual);
        public static readonly CompareOperatorKeyword GreaterThan = new CompareOperatorKeyword(CompareOperator.GreaterThan);
        public static readonly CompareOperatorKeyword LessThan = new CompareOperatorKeyword(CompareOperator.LessThan);
        public static readonly CompareOperatorKeyword GreaterThanOrEqual = new CompareOperatorKeyword(CompareOperator.GreaterThanOrEqual);
        public static readonly CompareOperatorKeyword LessThanOrEqual = new CompareOperatorKeyword(CompareOperator.LessThanOrEqual);
        public static readonly CompareOperatorKeyword StartsWith = new CompareOperatorKeyword(CompareOperator.StartsWith);
        public static readonly CompareOperatorKeyword EndsWith = new CompareOperatorKeyword(CompareOperator.EndsWith);
        public static readonly CompareOperatorKeyword Contains = new CompareOperatorKeyword(CompareOperator.Contains);

        public CompareOperator Type { get; }
        public override TokenType TokenType { get; } = TokenType.CompareOperator;

        private CompareOperatorKeyword(CompareOperator type)
        {
            Type = type;
        }
    }

    internal enum CompareOperator
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