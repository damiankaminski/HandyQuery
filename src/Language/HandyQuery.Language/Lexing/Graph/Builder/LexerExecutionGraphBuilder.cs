using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using HandyQuery.Language.Lexing.Grammar.Structure;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    // TODO: final graph optimization
    // TODO: or maybe runtime should be able to detect it? while moving back it could save items to the stack
    // e.g. 
    // 1. `<all-filters> : <filter> LogicalOperator <all-filters> | <filter>`
    // should be optimized to something like `<filter> optional (LogicalOperator <all-filters>)`
    // lexer should tokenize `<filter>` only once
    // 2. `<function-invokation> : FunctionName ParamsOpen ParamsClose | FunctionName ParamsOpen <params> ParamsClose`
    // should be optimized to something like `FunctionName ParamsOpen (ParamsClose | <params> ParamsClose)`
    // lexer should tokenize `FunctionName ParamsOpen` only once

    internal static class LexerExecutionGraphBuilder
    {
        public static RootNode BuildGraph(Grammar.Grammar grammar)
        {
            {
                var nonTerminalHeads = ProcessNonTerminalsHeads();
                ProcessRemainingNonTerminalsItems(nonTerminalHeads);

                var rootHead = nonTerminalHeads[grammar.Root.NonTerminalUsage.Impl];
                return new RootNode().WithChild(rootHead);
            }

            IReadOnlyDictionary<GrammarNonTerminal, Node> ProcessNonTerminalsHeads()
            {
                var nonTerminalHeads = new Dictionary<GrammarNonTerminal, Node>();

                foreach (var nonTerminal in grammar.NonTerminals)
                {
                    ProcessSingle(nonTerminal);
                }

                return nonTerminalHeads;

                Node ProcessSingle(GrammarNonTerminal nonTerminal)
                {
                    if (nonTerminalHeads.TryGetValue(nonTerminal, out var aleadyProcessed))
                    {
                        if (aleadyProcessed == null)
                        {
                            var msg = $"Recursive non-terminal {nonTerminal.Name} usage cannot be placed " +
                                      "on the very start of non-terminal body. It would cause " +
                                      "infinite recursion.";
                            throw new LexerExecutionGraphException(msg,
                                LexerExecutionGraphException.Id.InfiniteRecursion);
                        }

                        return aleadyProcessed;
                    }

                    // marks that processing of `nonTerminal` started
                    nonTerminalHeads.Add(nonTerminal, null);

                    var heads = new NodesCollection();

                    foreach (var orConditionOperand in nonTerminal.Body.OrConditionOperands)
                    {
                        var bodyItem = orConditionOperand[0];

                        switch (bodyItem)
                        {
                            case GrammarTerminalUsage terminalUsage:
                                var terminalNode = new TerminalNode(terminalUsage);
                                heads.Add(terminalNode);
                                break;

                            case GrammarNonTerminalUsage nonTerminalUsage:
                                var newNonTerminalHead = ProcessSingle(nonTerminalUsage.Impl);
                                var nonTerminalUsageNode =
                                    new NonTerminalUsageNode(nonTerminalUsage.Name, newNonTerminalHead);
                                heads.Add(nonTerminalUsageNode);
                                break;

                            default:
                                throw new LexerExecutionGraphException(
                                    $"Unknown non-terminal body item type: {bodyItem.Type}",
                                    LexerExecutionGraphException.Id.UnknownNonTerminalBodyItemType);
                        }
                    }

                    var head = heads.Count > 1 ? new BranchNode(heads) : heads[0];

                    if (nonTerminalHeads[nonTerminal] != null)
                    {
                        throw new LexerExecutionGraphException("Unexpected error.",
                            LexerExecutionGraphException.Id.UnexpectedError);
                    }
                       
                    nonTerminalHeads[nonTerminal] = head;
                    return head;
                }
            }

            void ProcessRemainingNonTerminalsItems(IReadOnlyDictionary<GrammarNonTerminal, Node> nonTerminalHeads)
            {
                foreach (var nonTerminal in grammar.NonTerminals)
                {
                    ProcessSingle(nonTerminal);
                }

                void ProcessSingle(GrammarNonTerminal nonTerminal)
                {
                    var head = nonTerminalHeads[nonTerminal];

                    for (var operandIndex = 0;
                        operandIndex < nonTerminal.Body.OrConditionOperands.Count;
                        operandIndex++)
                    {
                        var currentTail = head is BranchNode node
                            ? node.Heads.ToList()[operandIndex]
                            : head;

                        foreach (var bodyItem in nonTerminal.Body.OrConditionOperands[operandIndex].Skip(1))
                        {
                            switch (bodyItem)
                            {
                                case GrammarTerminalUsage terminalUsage:
                                    var terminalNode = new TerminalNode(terminalUsage);
                                    currentTail.AddChild(terminalNode);
                                    currentTail = terminalNode;
                                    break;

                                case GrammarNonTerminalUsage nonTerminalUsage:
                                    var newNonTerminalHead = nonTerminalHeads[nonTerminalUsage.Impl];
                                    var nonTerminalUsageNode =
                                        new NonTerminalUsageNode(nonTerminalUsage.Name, newNonTerminalHead);
                                    currentTail.AddChild(nonTerminalUsageNode);
                                    currentTail = nonTerminalUsageNode;
                                    break;

                                default:
                                    throw new LexerExecutionGraphException(
                                        $"Unknown non-terminal body item type: {bodyItem.Type}",
                                        LexerExecutionGraphException.Id.UnknownNonTerminalBodyItemType);
                            }
                        }
                    }
                }
            }
        }

        private sealed class NodesCollection : Collection<Node>
        {
            public override string ToString()
            {
                return $"[{string.Join(", ", this)}]";
            }
        }
    }
}