namespace HandyQuery.Language.Parsing.Grammar
{
    internal sealed class GrammarParserException : QueryLanguageException
    {
        public GrammarParserException(string message) : base(message)
        {
        }
    }
}