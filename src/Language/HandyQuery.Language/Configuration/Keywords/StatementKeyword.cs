using HandyQuery.Language.Lexing;

namespace HandyQuery.Language.Configuration.Keywords
{
    internal sealed class StatementKeyword : Keyword
    {
        public static readonly StatementKeyword Empty = new StatementKeyword(Statement.Empty);
        public static readonly StatementKeyword NotEmpty = new StatementKeyword(Statement.NotEmpty);
        public static readonly StatementKeyword IsTrue = new StatementKeyword(Statement.IsTrue);
        public static readonly StatementKeyword IsFalse = new StatementKeyword(Statement.IsFalse);

        public Statement Type { get; }
        public override TokenType TokenType { get; } = TokenType.Statement;

        private StatementKeyword(Statement type)
        {
            Type = type;
        }
    }

    internal enum Statement
    {
        Empty,
        NotEmpty,
        IsTrue,
        IsFalse
    }
}