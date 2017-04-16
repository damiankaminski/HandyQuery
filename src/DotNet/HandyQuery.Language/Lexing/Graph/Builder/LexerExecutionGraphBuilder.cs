using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public static RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();

            var currentNodes = new Nodes(root);
            var part = grammarRoot.PartUsage.Impl;

            ProcessPart(part, currentNodes, out var _);

            return root;
        }

        private static void ProcessPart(GrammarPart part, Nodes currentNodes, out Nodes leaveNodes)
        {
            leaveNodes = new Nodes();
            var context = new Context(part);
            var contextStack = new Stack<Context>();

            while (true)
            {
                if (context.MoveNext() == false)
                {
                    if (contextStack.Any() == false) break;

                    context = contextStack.Pop();
                    continue;
                }

                switch (context.CurrentBodyItem)
                {
                    case GrammarTokenizerUsage tokenizerUsage:
                    {
                        var node = new TokenizerNode(tokenizerUsage);
                        currentNodes.AddChild(node);
                        currentNodes = node;
                        leaveNodes = node;
                        break;
                    }

                    case GrammarPartUsage partUsage:
                    {
                        contextStack.Push(context);
                        context = new Context(partUsage.Impl);
                        continue;
                    }

                    case GrammarOrCondition orCondition:
                    {
                        leaveNodes = new Nodes();
                        foreach (var operand in orCondition.Operands)
                        {
                            switch (operand)
                            {
                                case GrammarTokenizerUsage tokenizerUsage:
                                    var node = new TokenizerNode(tokenizerUsage);
                                    currentNodes.AddChild(node);
                                    leaveNodes.Add(node);
                                    break;
                                case GrammarPartUsage partUsage:
                                    ProcessPart(partUsage.Impl, currentNodes, out var partLeaveNodes);
                                    leaveNodes.AddRange(partLeaveNodes);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        currentNodes = leaveNodes;
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private sealed class Nodes : List<Node>
        {
            public Nodes()
            {
            }

            /// <summary>
            /// Creates a new Nodes list with single value added.
            /// </summary>
            public Nodes(Node node)
            {
                Add(node);
            }

            public void AddChild(Node node)
            {
                foreach (var item in this)
                {
                    item.AddChildImpl(node);
                }
            }

            public static implicit operator Nodes(Node node)
            {
                return new Nodes(node);
            }
        }

        private sealed class Context
        {
            public Context(GrammarPart part)
            {
                Part = part;
            }

            public IGrammarBodyItem CurrentBodyItem => Part.Body[CurrentBodyItemIndex];

            private GrammarPart Part { get; }
            private int CurrentBodyItemIndex { get; set; } = -1;

            public bool MoveNext()
            {
                if (CurrentBodyItemIndex + 1 >= Part.Body.Count) return false;

                CurrentBodyItemIndex++;
                return true;
            }
        }
    }
}