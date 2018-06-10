namespace HandyQuery.Language.Parsing.StateTransition
{
    internal sealed class StateTransitionException : QueryLanguageException
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
        
        public StateTransitionException(string message, Id errorId) : base(message)
        {
            ErrorId = errorId;
        }
    }
}