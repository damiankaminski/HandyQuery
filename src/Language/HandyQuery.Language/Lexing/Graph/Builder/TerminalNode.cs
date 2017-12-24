using System;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class TerminalNode : Node
    {
        public readonly ITokenizer Tokenizer;
        public Node Child { get; private set; }
        
        public TerminalNode(GrammarTerminalUsage terminalUsage)
        {
            Tokenizer = terminalUsage?.Impl;
        }

        public TerminalNode WithChild(Node child)
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
            var name = Tokenizer?.GetType().Name;
            name = name?.Substring(0, name.LastIndexOf("Tokenizer", StringComparison.Ordinal)) ?? "ROOT";
            return name;
        }
    }
}