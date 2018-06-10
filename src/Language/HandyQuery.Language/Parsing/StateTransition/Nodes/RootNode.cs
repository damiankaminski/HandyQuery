namespace HandyQuery.Language.Parsing.StateTransition.Nodes
{
    internal sealed class RootNode : Node
    {
        public Node Child { get; private set; }

        public RootNode WithChild(Node child)
        {
            if (Child != null)
            {
                throw new StateTransitionException("Child change is not allowed.", 
                    StateTransitionException.Id.NodeChildChangeNotAllowed);
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