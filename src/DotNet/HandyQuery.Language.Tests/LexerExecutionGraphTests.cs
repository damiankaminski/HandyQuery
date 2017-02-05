using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph;
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
                        $AllFilters = ColumnName Statement
                        return $AllFilters
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                    CreateNode("ColumnName").AddChild(
                        CreateNode("Statement")))
                };
            }

            {
                yield return new TestCase("Part usage")
                {
                    Grammar = @"
                        $Value = Literal
                        $AllFilters = ColumnName CompareOperator $Value
                        return $AllFilters
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                        CreateNode("ColumnName").AddChild(
                            CreateNode("CompareOperator").AddChild(
                                CreateNode("Literal"))))
                };
            }

            {
                var groupClose = CreateNode("GroupClose", true);
                yield return new TestCase("Or usage with part")
                {
                    Grammar = @"
                        $Value = Literal

                        $Filter = ?GroupOpen $FilterWithCompareOp|$FilterWithStatement ?GroupClose
                        $FilterWithCompareOp = ColumnName CompareOperator $Value
                        $FilterWithStatement = ColumnName Statement

                        return $Filter
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                        CreateNode("GroupOpen", true).AddChild(
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
                var groupClose = CreateNode("GroupClose", true);
                yield return new TestCase("Or usage with tokenizers")
                {
                    Grammar = @"
                        $Filter = ?GroupOpen ColumnName|Statement ?GroupClose
                        return $Filter
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                        CreateNode("GroupOpen", true).AddChild(
                            CreateNode("ColumnName").AddChild(
                                groupClose)).AddChild(
                            CreateNode("Statement").AddChild(
                                groupClose)))
                };
            }

            {
                var groupClose = CreateNode("GroupClose", true);
                yield return new TestCase("Or usage with part and tokenizer")
                {
                    Grammar = @"
                        $Value = Literal

                        $Filter = ?GroupOpen $FilterWithCompareOp|Statement ?GroupClose
                        $FilterWithCompareOp = ColumnName CompareOperator $Value

                        return $Filter
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                        CreateNode("GroupOpen", true).AddChild(
                            CreateNode("ColumnName").AddChild(
                                CreateNode("CompareOperator").AddChild(
                                    CreateNode("Literal").AddChild(
                                        groupClose)))).AddChild(
                            CreateNode("Statement").AddChild(
                                groupClose)))
                };
            }

            {
                var literal = CreateNode("Literal");
                var paramsSeparator = CreateNode("ParamsSeparator");
                paramsSeparator.AddChild(literal);
                var @params = literal.AddChild(paramsSeparator);
                yield return new TestCase("Simple cycles")
                {
                    Grammar = @"
                        $Value = Literal

                        $FunctionInvokation = FunctionName ParamsOpen ?$Params ParamsClose
                        $Params = $Value ?$MoreParams
                        $MoreParams = ParamsSeparator $Params

                        return $FunctionInvokation
                    ",
                    ExpectedRoot = new Node(null).AddChild(
                        CreateNode("FunctionName").AddChild(
                            CreateNode("ParamsOpen").AddChild(
                                @params.AddChild(
                                    CreateNode("ParamsClose")))))
                };
            }
            
            // TODO: advanced cycles (cycles with or condition)
            /*
            $Value = Literal|$FunctionInvokation

            $FunctionInvokation = FunctionName ParamsOpen ?$Params ParamsClose
            $Params = $Value ?$MoreParams
            $MoreParams = ParamsSeparator $Params

            return $FunctionInvokation
            */
        }

        public sealed class TestCase
        {
            private string Name { get; }
            public string Grammar { get; set; }
            internal Node ExpectedRoot { get; set; }

            public TestCase(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return Name.Replace(' ', '_');
            }
        }

        private static Node CreateNode(string name, bool isOptional = false)
        {
            return new Node(CreateTokenizerUsage(name, isOptional));
        }

        private static GrammarTokenizerUsage CreateTokenizerUsage(string name, bool isOptional = false)
        {
            return new GrammarTokenizerUsage(name, isOptional, TokenizersSource.GetTokenizer(name));
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