namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class StatementKeyword : Keyword
    {
        public static readonly StatementKeyword Empty = new StatementKeyword(StatementType.Empty);
        public static readonly StatementKeyword NotEmpty = new StatementKeyword(StatementType.NotEmpty);
        public static readonly StatementKeyword IsTrue = new StatementKeyword(StatementType.IsTrue);
        public static readonly StatementKeyword IsFalse = new StatementKeyword(StatementType.IsFalse);

        public StatementType StatementType { get; }
        public override KeywordType KeywordType { get; } = KeywordType.Statement;

        private StatementKeyword(StatementType statementType)
        {
            StatementType = statementType;
        }

        public override string ToString()
        {
            return $"{KeywordType}: {StatementType.ToString()}";
        }
    }

    internal enum StatementType
    {
        Empty,
        NotEmpty,
        IsTrue,
        IsFalse
    }
}