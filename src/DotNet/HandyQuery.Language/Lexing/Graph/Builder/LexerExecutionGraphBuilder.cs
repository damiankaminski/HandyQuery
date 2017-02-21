using System;
using System.Collections.Generic;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph.Builder.Node;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    internal sealed class LexerExecutionGraphBuilder
    {
        public readonly RootNode Root = new RootNode();

        public sealed class VisitResult
        {
            public readonly List<BuilderNodeBase> LeaveNodes;
            public bool CycleDetected { get; set; }

            public VisitResult(List<BuilderNodeBase> leaveNodes)
            {
                LeaveNodes = leaveNodes;
            }
        }

        public void BuildGraph(GrammarPart grammarRoot)
        {
            Visit(new GrammarPartUsage(grammarRoot.Name, false, grammarRoot), new List<BuilderNodeBase> { Root });
        }

        public VisitResult Visit(GrammarTokenizerUsage tokenizerUsage, List<BuilderNodeBase> parents)
        {
            var node = new TokenizerNode(tokenizerUsage);
            node.AddParents(parents);
            return new VisitResult(new List<BuilderNodeBase> { node });
        }

        public VisitResult Visit(GrammarOrCondition orCondition, List<BuilderNodeBase> parents)
        {
            var leaveNodes = new List<BuilderNodeBase>();
            foreach (var operand in orCondition.Operands)
            {
                leaveNodes.AddRange(Visit(operand, parents).LeaveNodes);
            }

            return new VisitResult(leaveNodes);
        }

        public VisitResult Visit(GrammarPartUsage partUsage, List<BuilderNodeBase> parents)
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
    }
}