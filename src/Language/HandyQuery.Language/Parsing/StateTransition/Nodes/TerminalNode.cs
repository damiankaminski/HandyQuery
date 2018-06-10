using System;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Parsing.Grammar.Structure;

namespace HandyQuery.Language.Parsing.StateTransition.Nodes
{
    internal sealed class TerminalNode : Node
    {
        public readonly ITokenizer Tokenizer;
        public readonly string ArgumentValue;
        public Node Child { get; private set; }
        
        public TerminalNode(GrammarTerminalUsage terminalUsage)
        {
            Tokenizer = terminalUsage?.Impl;
            ArgumentValue = terminalUsage?.ArgumentValue;
        }

        public TerminalNode WithChild(Node child)
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
            var name = Tokenizer?.GetType().Name;
            name = name?.Substring(0, name.LastIndexOf("Tokenizer", StringComparison.Ordinal)) ?? "ROOT";
            return name;
        }
    }
}