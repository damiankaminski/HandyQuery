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

            // TODO: make sure it valides reference equality
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
                    ExpectedRoot = new RootNode().WithChild(
                        CreateNode("ColumnName").WithChild(
                            CreateNode("Statement")))
                };
            }

            {
                yield return new TestCase("Non-terminal usage")
                {
                    Grammar = @"
                        <value> ::= Literal
                        <all-filters> ::= ColumnName CompareOperator <value>
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        CreateNode("ColumnName").WithChild(
                            CreateNode("CompareOperator").WithChild(
                                CreateNode("Literal"))))
                };
            }

            {
                yield return new TestCase("Nested non-terminal usage")
                {
                    Grammar = @"
                        <test> ::= FunctionName
                        <value> ::= <test> Literal
                        <all-filters> ::= ColumnName CompareOperator <value> Statement
                        return <all-filters>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        CreateNode("ColumnName").WithChild(
                            CreateNode("CompareOperator").WithChild(
                                CreateNode("FunctionName").WithChild(
                                    CreateNode("Literal").WithChild(
                                        CreateNode("Statement"))))))
                };
            }

            {
                var groupClose = CreateNode("GroupClose");
                yield return new TestCase("Or usage")
                {
                    Grammar = @"
                        <value> ::= Literal

                        <filter> ::= GroupOpen <filter-body> GroupClose
                        <filter-body> ::= ColumnName CompareOperator <value> | ColumnName Statement

                        return <filter>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        CreateNode("GroupOpen").WithChild(
                            new BranchNode()
                                .AddChild(
                                    CreateNode("ColumnName").WithChild(
                                        CreateNode("CompareOperator").WithChild(
                                            CreateNode("Literal").WithChild(
                                                groupClose))))
                                .AddChild(
                                    CreateNode("ColumnName").WithChild(
                                        CreateNode("Statement").WithChild(
                                            groupClose)))))    
                };
            }
            
            {
                var paramsClose = CreateNode("ParamsClose");
                
                var paramsSeparator = CreateNode("ParamsSeparator");
                var literal1 = CreateNode("Literal").WithChild(paramsSeparator);
                var literal2 = CreateNode("Literal").WithChild(paramsClose);
                
                var paramsBranch = new BranchNode().AddChild(literal1).AddChild(literal2);
                paramsSeparator.WithChild(paramsBranch);
                
                yield return new TestCase("Simple recursion")
                {
                    Grammar = @"
                        <value> ::= Literal

                        <function-invokation> ::= FunctionName ParamsOpen <params> ParamsClose
                        <params> ::= <value> ParamsSeparator <params> | <value>

                        return <function-invokation>
                    ",
                    ExpectedRoot = new RootNode().WithChild(
                        CreateNode("FunctionName").WithChild(
                            CreateNode("ParamsOpen").WithChild(
                                paramsBranch)))
                };
            }
            
            {
                var paramsClose = CreateNode("ParamsClose");
                
                var paramsSeparator = CreateNode("ParamsSeparator");
                var valueBranch1 = new BranchNode().AddChild(CreateNode("Literal").WithChild(paramsSeparator));
                var valueBranch2 = new BranchNode().AddChild(CreateNode("Literal").WithChild(paramsClose));
                
                var paramsBranch = new BranchNode().AddChild(valueBranch1).AddChild(valueBranch2);
                paramsSeparator.WithChild(paramsBranch);

                var functionInvokation = CreateNode("FunctionName").WithChild(
                    CreateNode("ParamsOpen").WithChild(paramsBranch));

                valueBranch1.AddChild(functionInvokation);
                valueBranch2.AddChild(functionInvokation);
                
                yield return new TestCase("Deep recursion")
                {
                    Grammar = @"
                        <value> ::= Literal | <function-invokation>

                        <function-invokation> ::= FunctionName ParamsOpen <params> ParamsClose
                        <params> ::= <value> ParamsSeparator <params> | <value>

                        return <function-invokation>
                    ",
                    ExpectedRoot = new RootNode().WithChild(functionInvokation)
                };
            }
            
            // TODO: Unresolvable recursion (without or condition, should throw meaningful exception) - already implemented, only test is needed
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