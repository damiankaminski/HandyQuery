namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class StatementKeyword : KeywordBase
    {
        public static readonly StatementKeyword IsEmpty = new StatementKeyword(StatementType.IsEmpty);
        public static readonly StatementKeyword IsNotEmpty = new StatementKeyword(StatementType.IsNotEmpty);
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
        IsEmpty,
        IsNotEmpty,
        IsTrue,
        IsFalse
    }
}