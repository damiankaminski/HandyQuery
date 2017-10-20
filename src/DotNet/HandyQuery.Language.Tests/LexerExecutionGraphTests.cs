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
        private static readonly TokenizersSource TokenizersSource;

        static LexerExecutionGraphTests()
        {
            TokenizersSource = new TokenizersSource();
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void ShouldCreateProperGraph(TestCase testCase)
        {
            var expected = new LexerExecutionGraph(testCase.ExpectedRoot);

            var graph = CreateGraph(testCase.Grammar);

            graph.Equals(expected).Should().BeTrue();
        }

        private static IEnumerable<TestCase> GetTestCases()
        {
            {
                yield return new TestCase("Most simple grammar")
                {
                    Grammar = @"
                        <all-filters> ::= ColumnName Statement
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("ColumnName").AddChild(
                            CreateNode("Statement")))
                };
            }

            {
                yield return new TestCase("Part usage")
                {
                    Grammar = @"
                        <value> ::= Literal
                        <all-filters> ::= ColumnName CompareOperator <value>
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("ColumnName").AddChild(
                            CreateNode("CompareOperator").AddChild(
                                CreateNode("Literal"))))
                };
            }

            {
                yield return new TestCase("Nested nonTerminal usage")
                {
                    Grammar = @"
                        <test> ::= FunctionName
                        <value> ::= <test> Literal
                        <all-filters> ::= ColumnName CompareOperator <value> Statement
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("ColumnName").AddChild(
                            CreateNode("CompareOperator").AddChild(
                                CreateNode("FunctionName").AddChild(
                                    CreateNode("Literal").AddChild(
                                        CreateNode("Statement"))))))
                };
            }

            {
                var groupClose = CreateNode("GroupClose");
                yield return new TestCase("Or usage with nonTerminal")
                {
                    Grammar = @"
                        <value> ::= Literal

                        <filter> ::= GroupOpen <filter-with-compare-op> |<filter-with-statement> GroupClose
                        <filter-with-compare-op> ::= ColumnName CompareOperator <value>
                        <filter-with-statement> ::= ColumnName Statement

                        return <filter>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("GroupOpen").AddChild(
                            CreateNode("ColumnName").AddChild(
                                CreateNode("CompareOperator").AddChild(
                                    CreateNode("Literal").AddChild(
                                        groupClose)))).AddChild(
                            CreateNode("ColumnName").AddChild(
                                CreateNode("Statement").AddChild(
                                    groupClose))))
                };
            }

            {
                var groupClose = CreateNode("GroupClose");
                yield return new TestCase("Or usage with tokenizers")
                {
                    Grammar = @"
                        <filter> ::= GroupOpen ColumnName|Statement GroupClose
                        return <filter>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("GroupOpen").AddChild(
                            CreateNode("ColumnName").AddChild(
                                groupClose)).AddChild(
                            CreateNode("Statement").AddChild(
                                groupClose)))
                };
            }

            {
                var groupClose = CreateNode("GroupClose");
                yield return new TestCase("Or usage with nonTerminal and tokenizer")
                {
                    Grammar = @"
                        <value> ::= Literal

                        <filter> ::= GroupOpen <filter-with-compare-op> |Statement GroupClose
                        <filter-with-compare-op> ::= ColumnName CompareOperator <value>

                        return <filter>
                    ",
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("GroupOpen").AddChild(
                            CreateNode("ColumnName").AddChild(
                                CreateNode("CompareOperator").AddChild(
                                    CreateNode("Literal").AddChild(
                                        groupClose)))).AddChild(
                            CreateNode("Statement").AddChild(
                                groupClose)))
                };
            }

//            {
//                var paramsClose = CreateNode("ParamsClose");
//                
//                var literal = CreateNode("Literal");
//                var paramsSeparator = CreateNode("ParamsSeparator").AddChild(literal);
//                var @params = literal.AddChild(paramsSeparator).AddChild(paramsClose);
//
//                yield return new TestCase("Simple cycles")
//                {
//                    Grammar = @"
//                        <value> ::= Literal
//
//                        <function-invokation> ::= FunctionName ParamsOpen <params> ParamsClose
//                        <params> ::= <value> ?<more-params>
//                        <more-params> ::= ParamsSeparator <params>
//
//                        return <function-invokation>
//                    ",
//                    ExpectedRoot = new RootNode().AddChild(
//                        CreateNode("FunctionName").AddChild(
//                            CreateNode("ParamsOpen").AddChild(
//                                @params.AddChild(
//                                    paramsClose))))
//                };
//            }

            /*{
                var paramsClose = CreateNode("ParamsClose");
                var paramsOpen = CreateNode("ParamsOpen")
                    .AddChild(CreateNode("Literal").AddChild(paramsClose))
                    .AddChild(paramsClose);
                var functionName = CreateNode("FunctionName")
                    .AddChild(paramsOpen);
                paramsOpen.AddChild(functionName);
                yield return new TestCase("Cycles with an or condition")
                {
                    Grammar = @"
                        <value> ::= Literal|$FunctionInvokation

                        <function-invokation> ::= FunctionName ParamsOpen ?<params> ParamsClose
                        <params> ::= <value>

                        return $FunctionInvokation
                    ",
                    ExpectedRoot = new Node().AddChild(functionName)
                };
            }*/
        }

        public sealed class TestCase
        {
            private string Name { get; }
            public string Grammar { get; set; }
            internal RootNode ExpectedRoot { get; set; }

            public TestCase(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return Name.Replace(' ', '_');
            }
        }

        private static TerminalNode CreateNode(string name)
        {
            return new TerminalNode(new GrammarTerminalUsage(name, TokenizersSource.GetTokenizer(name)));
        }

        private static LexerExecutionGraph CreateGraph(string grammar)
        {
            var reader = new LexerStringReader(grammar, 0);
            var parser = new LexerGenerator.ParserImpl(reader, TokenizersSource);
            var root = parser.Parse();

            return LexerExecutionGraph.Build(root);
        }
    }
}