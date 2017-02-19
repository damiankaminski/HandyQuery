using System;
using System.Collections.Generic;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public readonly BuilderNode Root = new BuilderNode(null);

        public sealed class VisitResult
        {
            public readonly List<BuilderNode> LeaveNodes;
            public bool CycleDetected { get; set; }

            public VisitResult(List<BuilderNode> leaveNodes)
            {
                LeaveNodes = leaveNodes;
            }
        }

        public void BuildGraph(GrammarPart grammarRoot)
        {
            Visit(new GrammarPartUsage(grammarRoot.Name, false, grammarRoot), new List<BuilderNode> { Root });
        }

        public VisitResult Visit(GrammarTokenizerUsage tokenizerUsage, List<BuilderNode> parents)
        {
            var node = new BuilderNode(tokenizerUsage);
            node.AddParents(parents);
            return new VisitResult(new List<BuilderNode> { node });
        }

        public VisitResult Visit(GrammarOrCondition orCondition, List<BuilderNode> parents)
        {
            var leaveNodes = new List<BuilderNode>();
            foreach (var operand in orCondition.Operands)
            {
                leaveNodes.AddRange(Visit(operand, parents).LeaveNodes);
            }

            return new VisitResult(leaveNodes);
        }

        public VisitResult Visit(GrammarPartUsage partUsage, List<BuilderNode> parents)
        {
            var nodes = parents;
            var body = partUsage.Impl.Body;

            for (var i = 0; i < body.Count; i++)
            {
                var item = body[i];
                var visitResult = Visit(item, nodes);
                nodes = visitResult.LeaveNodes;
            }

            return new VisitResult(nodes);
        }

        private VisitResult Visit(IGrammarElement any, List<BuilderNode> parents)
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
    }
}