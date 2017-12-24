namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class RootNode : Node
    {
        public Node Child { get; private set; }

        public RootNode WithChild(Node child)
        {
            if (Child != null)
            {
                throw new LexerExecutionGraphException("Child change is not allowed.", 
                    LexerExecutionGraphException.Id.NodeChildChangeNotAllowed);
            }
            
            Child = child;
            return this;
        }

        public override string ToString()
        {
            return "ROOT";
        }
    }
}