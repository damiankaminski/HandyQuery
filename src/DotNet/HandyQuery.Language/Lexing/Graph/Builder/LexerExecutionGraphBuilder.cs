using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph.Builder.Node;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public RootNode BuildGraph(GrammarReturn grammarRoot)
        {
            var root = new RootNode();
            Visit(grammarRoot.PartUsage, new List<BuilderNodeBase> { root });
            // TODO: create optional edges

            return root;
        }

        private VisitResult Visit(GrammarTokenizerUsage tokenizerUsage, List<BuilderNodeBase> parents)
        {
            var node = new TokenizerNode(tokenizerUsage);
            node.AddParents(parents);
            return new VisitResult(new List<BuilderNodeBase> { node });
        }

        private VisitResult Visit(GrammarOrCondition orCondition, List<BuilderNodeBase> parents)
        {
            var leaveNodes = new List<BuilderNodeBase>();
            foreach (var operand in orCondition.Operands)
            {
                leaveNodes.AddRange(Visit(operand, parents).LeaveNodes);
            }

            return new VisitResult(leaveNodes);
        }

        private VisitResult Visit(GrammarPartUsage partUsage, List<BuilderNodeBase> parents)
        {
            var partNode = new PartUsageNode(partUsage.IsOptional);
            partNode.AddParents(parents);
            var partNodes = new List<BuilderNodeBase> { partNode };

            if (partUsage.Impl.Body.Any())
            {
                var visitResult = Visit(partUsage.Impl.Body[0], new List<BuilderNodeBase>());
                parents = visitResult.LeaveNodes;
                partNode.EntryNodes = visitResult.LeaveNodes;

                foreach (var item in partUsage.Impl.Body.Skip(1))
                {
                    visitResult = Visit(item, parents);
                    parents = visitResult.LeaveNodes;
                }
            }

            return new VisitResult(partNodes);
        }

        private VisitResult Visit(IGrammarElement any, List<BuilderNodeBase> parents)
        {
            switch (any.Type)
            {
                case GrammarElementType.TokenizerUsage:
                    return Visit((GrammarTokenizerUsage)any, parents);
                case GrammarElementType.OrCondition:
                    return Visit((GrammarOrCondition)any, parents);
                case GrammarElementType.PartUsage:
                    return Visit((GrammarPartUsage)any, parents);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private sealed class VisitResult
        {
            public readonly List<BuilderNodeBase> LeaveNodes;

            public VisitResult(List<BuilderNodeBase> leaveNodes)
            {
                LeaveNodes = leaveNodes;
            }
        }
    }
}