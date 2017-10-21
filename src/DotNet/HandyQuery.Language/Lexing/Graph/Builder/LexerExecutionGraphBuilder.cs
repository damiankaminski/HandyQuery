using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        private readonly Stack<ProcessContext> _contexts = new Stack<ProcessContext>();
        
        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            Process(root, grammarRoot.NonTerminalUsage.Impl);
            
            return root;
        }

        private ProcessContext Process(NodesCollection tail, GrammarNonTerminal nonTerminal)
        {
            var recursiveContext = _contexts.FirstOrDefault(x => ReferenceEquals(x.NonTerminal, nonTerminal));
            if (recursiveContext != null)
            {
                // deep recursion
                var upperContext = _contexts.Peek();
                if (upperContext.Head == null)
                {
                    throw new LexerExecutionGraphException("Not allowed recursion type detected.");
                }
                
                upperContext.Head.AddChild(recursiveContext.Head);
                return recursiveContext;
            }
            
            var context = new ProcessContext(nonTerminal);
            _contexts.Push(context);
            
            if (nonTerminal.Body.Operands.Count > 1)
            {
                var branch = new BranchNode();
                context.Head = branch;
                tail.AddChild(branch);
                tail = new NodesCollection(branch);
            }
            
            foreach (var body in nonTerminal.Body.Operands)
            {
                var currentTail = tail;
                var hasTail = true;

                for (var index = 0; index < body.Count; index++)
                {
                    var bodyItem = body[index];
                    
                    switch (bodyItem)
                    {
                        case GrammarTerminalUsage terminalUsage:
                            var terminalNode = new TerminalNode(terminalUsage);
                            currentTail.AddChild(terminalNode);
                            currentTail = terminalNode;
                            
                            if (index == 0 && context.Head == null)
                            {
                                context.Head = terminalNode;
                            }
                            break;

                        case GrammarNonTerminalUsage nonTerminalUsage:
                            if (ReferenceEquals(nonTerminalUsage.Impl, nonTerminal))
                            {
                                // simple recursion (non-terminal used directly in itself)

                                if (nonTerminal.Body.Operands.Count < 2)
                                {
                                    throw new LexerExecutionGraphException("Infinite recursion detected. " +
                                                                           "Use '|' to create escape path.");
                                }

                                currentTail.AddChildren(tail);
                                hasTail = false; // cyclic operands do not have tails
                                break;
                            }

                            var tmpContext = Process(currentTail, nonTerminalUsage.Impl);
                            currentTail = tmpContext.Tail;
                            
                            if (index == 0 && context.Head == null)
                            {
                                context.Head = tmpContext.Head;
                            }
                            break;

                        default:
                            throw new LexerExecutionGraphException(
                                $"Unknown non-terminal body item type: {bodyItem.Type}");
                    }
                }

                if (hasTail)
                {
                    context.Tail.AddRange(currentTail);                    
                }
            }

            _contexts.Pop();
            return context;
        }

        private sealed class ProcessContext
        {
            public readonly GrammarNonTerminal NonTerminal;

            public Node Head { get; set; }

            public readonly NodesCollection Tail = new NodesCollection();

            public ProcessContext(GrammarNonTerminal nonTerminal)
            {
                NonTerminal = nonTerminal;
            }

            public override string ToString()
            {
                return NonTerminal.ToString();
            }
        }
        
        private sealed class NodesCollection : Collection<Node>
        {
            public NodesCollection()
            {
            }

            /// <summary>
            /// Creates a new Nodes list with single value added.
            /// </summary>
            public NodesCollection(Node node)
            {
                Add(node);
            }

            /// <summary>
            /// Creates a new Nodes list with multiple values added.
            /// </summary>
            public NodesCollection(IEnumerable<Node> nodes)
            {
                foreach (var node in nodes)
                {
                    Add(node);
                }
            }

            public void AddChild(Node node)
            {
                foreach (var item in this)
                {
                    item.AddChild(node);
                }
            }

            public void AddChildren(NodesCollection nodes)
            {
                foreach (var node in nodes)
                {
                    AddChild(node);
                }
            }

            public static implicit operator NodesCollection(Node node)
            {
                return new NodesCollection(node);
            }

            public override string ToString()
            {
                return $"[{string.Join(", ", this)}]";
            }

            public void AddRange(NodesCollection nodes)
            {
                foreach (var node in nodes)
                {
                    Add(node);
                }
            }
        }
    }
}