namespace HandyQuery.Language.Lexing.Graph
{
    internal sealed class LexerExecutionGraphException : QueryLanguageException
    {
        public readonly Id ErrorId;

        public enum Id
        {
            InfiniteRecursion = 1,
            UnknownNonTerminalBodyItemType,
            NodeChildChangeNotAllowed,
            UnknownNodeType,
            UnexpectedError
        }
        
        public LexerExecutionGraphException(string message, Id errorId) : base(message)
        {
            ErrorId = errorId;
        }
    }
}