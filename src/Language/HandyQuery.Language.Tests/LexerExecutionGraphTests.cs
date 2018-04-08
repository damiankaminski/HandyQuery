using System;
using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph;
using HandyQuery.Language.Lexing.Graph.Builder;
using NUnit.Framework;

namespace HandyQuery.Language.Tests
{
    public sealed class LexerExecutionGraphTests
    {
        private static readonly TokenizersSource TokenizersSource = new TokenizersSource();

        [TestCaseSource(nameof(GetIsEquivalentToTestCases))]
        public void IsEquivalentTo_Should_CompareGraphs(IsEquivalentToTestCase testCase)
        {
            var graphA = new LexerExecutionGraph(new RootNode().WithChild(testCase.A));
            var graphB = new LexerExecutionGraph(new RootNode().WithChild(testCase.B));

            graphA.IsEquivalentTo(graphB).Should().Be(testCase.AreEquivalent);
        }

        private static IEnumerable<IsEquivalentToTestCase> GetIsEquivalentToTestCases()
        {
            {
                yield return new IsEquivalentToTestCase("Simple")
                {
                    AreEquivalent = true,
                    A = Terminal("ColumnName").WithChild(Terminal("Literal")),
                    B = Terminal("ColumnName").WithChild(Terminal("Literal"))
                };
            }
            
            {
                yield return new IsEquivalentToTestCase("Non-terminal usage")
                {
                    AreEquivalent = true,
                    A = Terminal("ColumnName").WithChild(
                            Terminal("CompareOperator").WithChild(
                                new NonTerminalUsageNode("<value>", Terminal("Literal")))),
                    B = Terminal("ColumnName").WithChild(
                            Terminal("CompareOperator").WithChild(
                                new NonTerminalUsageNode("<value>", Terminal("Literal"))))
                };
            }
            
            {
                yield return new IsEquivalentToTestCase("Nested non-terminal usage")
                {
                    AreEquivalent = true,
                    A = Create(),
                    B = Create()
                };
                
                Node Create()
                {
                    var testNonTerminalUsage = new NonTerminalUsageNode("<test>", Terminal("FunctionName"))
                        .WithChild(Terminal("Literal"));
                    var valueNonTerminalUsage = new NonTerminalUsageNode("<value>", testNonTerminalUsage)
                        .WithChild(Terminal("Statement"));

                    return Terminal("ColumnName").WithChild(
                        Terminal("CompareOperator").WithChild(
                            valueNonTerminalUsage));
                }
            }
            
            {
                yield return new IsEquivalentToTestCase("Simple recursion")
                {
                    AreEquivalent = true,
                    A = Create(),
                    B = Create()
                };
                
                Node Create()
                {
                    var valueHead = Terminal("Literal");

                    var paramsHead = new BranchNode();
                    paramsHead
                        .AddHead(new NonTerminalUsageNode("<value>", valueHead).WithChild(
                            Terminal("ParamsSeparator").WithChild(
                                new NonTerminalUsageNode("<params>", paramsHead))))
                        .AddHead(new NonTerminalUsageNode("<value>", valueHead));

                    var funcInvokeHead =
                        Terminal("FunctionName").WithChild(
                            Terminal("ParamsOpen").WithChild(
                                new NonTerminalUsageNode("<params>", paramsHead).WithChild(
                                    Terminal("ParamsClose"))));

                    return new NonTerminalUsageNode("<function-invokation>", funcInvokeHead);
                }
            }
            
            {
                yield return new IsEquivalentToTestCase("Wrong non-terminal reuse")
                {
                    AreEquivalent = false,
                    A = CreateA(),
                    B = CreateB()
                };
                
                Node CreateA()
                {
                    var nonTerminalHead = Terminal("CompareOperator");
                    return Terminal("ColumnName").WithChild(
                        new NonTerminalUsageNode("<comp>", nonTerminalHead).WithChild(
                            Terminal("Literal").WithChild(
                                new NonTerminalUsageNode("<comp>", nonTerminalHead))));
                }
                
                Node CreateB()
                {
                    var nonTerminalHead = Terminal("CompareOperator");
                    var nonTerminalHead2 = Terminal("CompareOperator");
                    return Terminal("ColumnName").WithChild(
                        new NonTerminalUsageNode("<comp>", nonTerminalHead).WithChild(
                            Terminal("Literal").WithChild(
                                new NonTerminalUsageNode("<comp>", nonTerminalHead2))));
                }
            }
            
            {
                yield return new IsEquivalentToTestCase("Wrong terminal name")
                {
                    AreEquivalent = false,
                    A = Terminal("ColumnName").WithChild(Terminal("Literal")),
                    B = Terminal("ColumnName").WithChild(Terminal("CompareOperator"))
                };
            }
        }

        public sealed class IsEquivalentToTestCase
        {
            private string Name { get; }
            public bool AreEquivalent { get; set; }
            internal Node A { get; set; }
            internal Node B { get; set; }

            public IsEquivalentToTestCase(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return Name.Replace(' ', '_');
            }
        }
        
        [TestCaseSource(nameof(GetBuildTestCases))]
        public void Build_Should_CreateProperGraph(BuildTestCase testCase)
        {
            if (testCase.ErrorId != null)
            {
                Action run = () => CreateGraph(testCase.Grammar);
                run.Should().ThrowExactly<LexerExecutionGraphException>().Where(x => x.ErrorId == testCase.ErrorId);
            }
            else
            {
                var expected = new LexerExecutionGraph(testCase.ExpectedRoot);
                
                var graph = CreateGraph(testCase.Grammar);
                
                graph.IsEquivalentTo(expected).Should().BeTrue();                
            }
        }

        private static IEnumerable<BuildTestCase> GetBuildTestCases()
        {
            {
                yield return new BuildTestCase("Most simple grammar")
                {
                    Grammar = @"
                        <all-filters> : ColumnName Statement
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        Terminal("ColumnName").WithChild(
                            Terminal("Statement")))
                };
            }

            {
                yield return new BuildTestCase("Non-terminal usage")
                {
                    Grammar = @"
                        <value> : Literal
                        <all-filters> : ColumnName CompareOperator <value>
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        Terminal("ColumnName").WithChild(
                            Terminal("CompareOperator").WithChild(
                                new NonTerminalUsageNode("<value>", Terminal("Literal")))))
                };
            }

            {
                var testNonTerminalUsage = new NonTerminalUsageNode("<test>", Terminal("FunctionName"))
                    .WithChild(Terminal("Literal"));
                var valueNonTerminalUsage = new NonTerminalUsageNode("<value>", testNonTerminalUsage)
                    .WithChild(Terminal("Statement"));

                yield return new BuildTestCase("Nested non-terminal usage")
                {
                    Grammar = @"
                        <test> : FunctionName
                        <value> : <test> Literal
                        <all-filters> : ColumnName CompareOperator <value> Statement
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        Terminal("ColumnName").WithChild(
                            Terminal("CompareOperator").WithChild(
                                valueNonTerminalUsage)))
                };
            }

            {
                var valueNonTerminalUsage = new NonTerminalUsageNode("<value>", Terminal("Literal"));
                var filterBodyNonTerminalUsage = new NonTerminalUsageNode("<filter-body>", new BranchNode()
                    .AddHead(
                        Terminal("ColumnName").WithChild(
                            Terminal("CompareOperator").WithChild(
                                valueNonTerminalUsage)))
                    .AddHead(
                        Terminal("ColumnName").WithChild(
                            Terminal("Statement"))));

                yield return new BuildTestCase("Or usage")
                {
                    Grammar = @"
                        <value> : Literal

                        <filter> : GroupOpen <filter-body> GroupClose
                        <filter-body> : ColumnName CompareOperator <value> | ColumnName Statement

                        return <filter>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        Terminal("GroupOpen")
                            .WithChild(filterBodyNonTerminalUsage.WithChild(Terminal("GroupClose"))))
                };
            }

            {
                var valueHead = Terminal("Literal");

                var paramsHead = new BranchNode();
                paramsHead
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead).WithChild(
                        Terminal("ParamsSeparator").WithChild(
                            new NonTerminalUsageNode("<params>", paramsHead))))
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead));

                var funcInvokeHead =
                    Terminal("FunctionName").WithChild(
                        Terminal("ParamsOpen").WithChild(
                            new NonTerminalUsageNode("<params>", paramsHead).WithChild(
                                Terminal("ParamsClose"))));

                yield return new BuildTestCase("Simple recursion")
                {
                    Grammar = @"
                        <value> : Literal

                        <function-invokation> : FunctionName ParamsOpen <params> ParamsClose
                        <params> : <value> ParamsSeparator <params> | <value>

                        return <function-invokation>
                    ",
                    ExpectedRoot = new RootNode().WithChild(funcInvokeHead)
                };
            }

            {
                var valueHead = new BranchNode().AddHead(Terminal("Literal"));
                
                var paramsHead = new BranchNode();
                paramsHead
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead).WithChild(
                        Terminal("ParamsSeparator").WithChild(
                            new NonTerminalUsageNode("<params>", paramsHead))))
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead));
                
                var funcInvokeHead = Terminal("FunctionName").WithChild(
                    Terminal("ParamsOpen").WithChild(
                        new NonTerminalUsageNode("<params>", paramsHead).WithChild(
                            Terminal("ParamsClose"))));

                valueHead.AddHead(new NonTerminalUsageNode("<function-invokation>", funcInvokeHead));
                
                yield return new BuildTestCase("Deep recursion")
                {
                    Grammar = @"
                        <value> : Literal | <function-invokation>

                        <function-invokation> : FunctionName ParamsOpen <params> ParamsClose
                        <params> : <value> ParamsSeparator <params> | <value>

                        return <function-invokation>
                    ",
                    ExpectedRoot = new RootNode().WithChild(funcInvokeHead)
                };
            }

            {
                var valueHead = new BranchNode().AddHead(Terminal("Literal"));
                
                var paramsHead = new BranchNode();
                paramsHead
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead).WithChild(
                        Terminal("ParamsSeparator").WithChild(
                            new NonTerminalUsageNode("<params>", paramsHead))))
                    .AddHead(new NonTerminalUsageNode("<value>", valueHead));
                
                var funcInvokeHead = Terminal("FunctionName").WithChild(
                    Terminal("ParamsOpen").WithChild(
                        new NonTerminalUsageNode("<params>", paramsHead).WithChild(
                            Terminal("ParamsClose"))));

                valueHead.AddHead(new NonTerminalUsageNode("<function-invokation>", funcInvokeHead));

                var filterWithCompareOpHead = Terminal("ColumnName").WithChild(
                    Terminal("CompareOperator").WithChild(
                        new NonTerminalUsageNode("<value>", valueHead)));
                
                yield return new BuildTestCase("Complex deep recursion")
                {
                    Grammar = @"
                        <value> : Literal | <function-invokation>

                        <filter-with-compare-op> : ColumnName CompareOperator <value>

                        <function-invokation> : FunctionName ParamsOpen <params> ParamsClose
                        <params> : <value> ParamsSeparator <params> | <value>

                        return <filter-with-compare-op>
                    ",
                    ExpectedRoot = new RootNode().WithChild(filterWithCompareOpHead)
                };
            }
            
            {
                yield return new BuildTestCase("Direct head recursive non terminal in or condition")
                {
                    Grammar = @"
                        <value> : Literal | <value>

                        return <value>
                    ",
                    ErrorId = LexerExecutionGraphException.Id.InfiniteRecursion
                };
            }
            
            {
                yield return new BuildTestCase("Deep head recursive non terminal")
                {
                    Grammar = @"
                        <some> : <value>

                        <date> : <some>

                        <value> : <date>

                        return <value>
                    ",
                    ErrorId = LexerExecutionGraphException.Id.InfiniteRecursion
                };
            }
            
            {
                yield return new BuildTestCase("Deep head recursive non terminal in or condition")
                {
                    Grammar = @"
                        <date> : <value>

                        <value> : Literal | <date>

                        return <value>
                    ",
                    ErrorId = LexerExecutionGraphException.Id.InfiniteRecursion
                };
            }
            
            {
                yield return new BuildTestCase("Deep head recursive non terminal in deep or condition")
                {
                    Grammar = @"
                        <some> : Literal | <value>

                        <date> : <some>

                        <value> : <date>

                        return <value>
                    ",
                    ErrorId = LexerExecutionGraphException.Id.InfiniteRecursion
                };
            }
            
            // TODO: test for recursion outside of head
        }

        public sealed class BuildTestCase
        {
            private string Name { get; }
            public string Grammar { get; set; }
            internal RootNode ExpectedRoot { get; set; }
            internal LexerExecutionGraphException.Id? ErrorId { get; set; }

            public BuildTestCase(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                var prefix = ErrorId != null ? "Exception: " : "";
                return $"{prefix}{Name}";
            }
        }
        
        private static TerminalNode Terminal(string name)
        {
            return new TerminalNode(new GrammarTerminalUsage(name, TokenizersSource.GetTokenizer(name)));
        }

        private static LexerExecutionGraph CreateGraph(string grammarText)
        {
            var parser = new GrammarParser(grammarText, TokenizersSource);
            var grammar = parser.Parse();

            return LexerExecutionGraph.Build(grammar);
        }
    }
}