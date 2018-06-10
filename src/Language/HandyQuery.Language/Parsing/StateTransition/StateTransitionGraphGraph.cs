using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Parsing.StateTransition.Nodes;

namespace HandyQuery.Language.Parsing.StateTransition
{
    internal sealed class StateTransitionGraph
    {
        internal readonly RootNode Root;

        internal StateTransitionGraph(RootNode root)
        {
            Root = root;
        }

        public static StateTransitionGraph Build(Grammar.Grammar grammar)
        {
            var root = StateTransitionGraphBuilder.BuildGraph(grammar);
            return new StateTransitionGraph(root);
        }

        public bool IsEquivalentTo(StateTransitionGraph expected)
        {
            var thisNodes = EnumerateInnerNodes(Root, new HashSet<Node>()).ToList();
            var expectedNodes = EnumerateInnerNodes(expected.Root, new HashSet<Node>()).ToList();

            // unique nodes are used to check if nodes are properly reused instead of copied with same content
            var thisUniqueNodes = new HashSet<Node>(thisNodes.Select(x => x.Node));
            var expectedUniqueNodes = new HashSet<Node>(expectedNodes.Select(x => x.Node));
            
            if (thisNodes.Count != expectedNodes.Count || thisUniqueNodes.Count != expectedUniqueNodes.Count)
            {
                return false;
            }

            var visitedNodesA = new HashSet<Node>();
            var visitedNodesB = new HashSet<Node>();
            
            return Compare(Root, expected.Root);
            
            bool Compare(Node a, Node b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                
                var visitedA = visitedNodesA.Contains(a);
                var visitedB = visitedNodesB.Contains(b);
            
                if (visitedA && visitedB) return true;
                if (visitedA != visitedB) return false;

                visitedNodesA.Add(a);
                visitedNodesB.Add(b);
            
                if (a.GetType() != b.GetType())
                {
                    return false;
                }

                switch (a)
                {
                    case RootNode na:
                    {
                        var nb = (RootNode)b;
                        return Compare(na.Child, nb.Child);
                    }

                    case TerminalNode na:
                    {
                        var nb = (TerminalNode)b;
                        if (na.Tokenizer.GetType().FullName != nb.Tokenizer.GetType().FullName) return false;
                        return Compare(na.Child, nb.Child);
                    }
                    case BranchNode na:
                    {
                        var nb = (BranchNode)b;
                        var naHeads = na.Heads.ToList();
                        var nbHeads = nb.Heads.ToList();

                        if (naHeads.Count != nbHeads.Count) return false;
                        
                        for (var i = 0; i < naHeads.Count; i++)
                        {
                            var aHead = naHeads[i];
                            var bHead = nbHeads[i];

                            if (Compare(aHead, bHead) == false) return false;
                        }
                        break;
                    }
                    case NonTerminalUsageNode na:
                    {
                        var nb = (NonTerminalUsageNode)b;
                        if (na.Name != nb.Name) return false;
                        if (Compare(na.Head, nb.Head) == false) return false;
                        if (Compare(na.Child, nb.Child) == false) return false;
                        break;
                    }
                    default:
                        throw new ArgumentException($"Invalid node type: {a?.GetType()}", nameof(a));
                }

                return true;
            }
            
            IEnumerable<(Node Node, bool Recursive)> EnumerateInnerNodes(Node node, HashSet<Node> visitedNodes)
            {
                if (visitedNodes.Contains(node))
                {
                    yield return (node, true);
                }
                else
                {
                    visitedNodes.Add(node);
                    yield return (node, false);
                
                    switch (node)
                    {
                        case RootNode n:
                            if(n.Child != null)
                                foreach (var innerNode in EnumerateInnerNodes(n.Child, visitedNodes))
                                    yield return innerNode;
                            break;
                        case TerminalNode n:
                            if(n.Child != null)
                                foreach (var innerNode in EnumerateInnerNodes(n.Child, visitedNodes))
                                    yield return innerNode;
                            break;
                        case BranchNode n:
                            foreach (var head in n.Heads)
                            foreach (var innerNode in EnumerateInnerNodes(head, visitedNodes))
                                yield return innerNode;
                            break;
                        case NonTerminalUsageNode n:
                            foreach (var innerNode in EnumerateInnerNodes(n.Head, visitedNodes))
                                yield return innerNode;
                            if(n.Child != null)
                                foreach (var innerNode in EnumerateInnerNodes(n.Child, visitedNodes))
                                    yield return innerNode;
                            break;
                        default:
                            throw new ArgumentException($"Invalid node type: {node?.GetType()}", nameof(node));
                    }
                }
            }
        }
    }
}