namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class GrammarParserException : QueryLanguageException
    {
        public GrammarParserException(string message) : base(message)
        {
        }
    }
}