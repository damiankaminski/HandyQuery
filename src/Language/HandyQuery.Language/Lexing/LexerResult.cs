namespace HandyQuery.Language.Lexing
{
    internal sealed class LexerResult
    {
        public LexerResult()
        {
            Tokens = new TokenList(); // TODO: pool?
        }

        public TokenList Tokens { get; }
    }
}