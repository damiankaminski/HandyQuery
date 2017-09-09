﻿using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph;
using HandyQuery.Language.Lexing.Graph.Builder;
using NUnit.Framework;

namespace HandyQuery.Language.Tests
{
    // TODO: get rid of a lot of Equals methods, they are not needed anymore since Node is using ITokenizer instead of TokenizerUsage

    public sealed class LexerExecutionGraphTests
    {
        private static readonly TokenizersSource _tokenizersSource;

        static LexerExecutionGraphTests()
        {
            _tokenizersSource = new TokenizersSource();
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
                    ExpectedRoot = new RootNode().AddChild(
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
                    ExpectedRoot = new RootNode().AddChild(
                        CreateNode("ColumnName").AddChild(
                            CreateNode("CompareOperator").AddChild(
                                CreateNode("Literal"))))
                };
            }

            {
                yield return new TestCase("Nested part usage")
                {
                    Grammar = @"
                        $Test = FunctionName
                        $Value = $Test Literal
                        $AllFilters = ColumnName CompareOperator $Value Statement
                        return $AllFilters
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
                yield return new TestCase("Or usage with part")
                {
                    Grammar = @"
                        $Value = Literal

                        $Filter = GroupOpen $FilterWithCompareOp|$FilterWithStatement GroupClose
                        $FilterWithCompareOp = ColumnName CompareOperator $Value
                        $FilterWithStatement = ColumnName Statement

                        return $Filter
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
                        $Filter = GroupOpen ColumnName|Statement GroupClose
                        return $Filter
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
                yield return new TestCase("Or usage with part and tokenizer")
                {
                    Grammar = @"
                        $Value = Literal

                        $Filter = GroupOpen $FilterWithCompareOp|Statement GroupClose
                        $FilterWithCompareOp = ColumnName CompareOperator $Value

                        return $Filter
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

            {
                var literal = CreateNode("Literal");
                var columnName = CreateNode("ColumnName")
                    .AddChild(CreateNode("CompareOperator", true).AddChild(literal))
                    .AddChild(literal);

                yield return new TestCase("Optional tokenizer")
                {
                    Grammar = @"
                        $AllFilters = ColumnName ?CompareOperator Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = new RootNode().AddChild(columnName)
                };
            }

            {
                var literal = CreateNode("Literal");
                var something = CreateNode("Statement").AddChild(
                    CreateNode("CompareOperator").AddChild(literal));

                var columnName = CreateNode("ColumnName")
                    .AddChild(something)
                    .AddChild(literal);

                yield return new TestCase("Optional part")
                {
                    Grammar = @"
                        $Something = Statement CompareOperator
                        $AllFilters = ColumnName ?$Something Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = new RootNode().AddChild(columnName)
                };
            }

//            {
//                var literal = CreateNode("Literal");
//                
//                var something = CreateNode("Statement").AddChild(
//                    CreateNode("CompareOperator").AddChild(literal));
//
//                var something2 = CreateNode("GroupOpen").AddChild(
//                    CreateNode("GroupClose").AddChild(literal));
//                
//                var columnName = CreateNode("ColumnName")
//                    .AddChild(something)
//                    .AddChild(something2)
//                    .AddChild(literal);
//
//                yield return new TestCase("Optional or condition")
//                {
//                    Grammar = @"
//                        $Something = Statement CompareOperator
//                        $Something2 = GroupOpen GroupClose
//                        $AllFilters = ColumnName ?$Something|?$Something2 Literal
//                        return $AllFilters
//                    ",
//                    ExpectedRoot = new RootNode().AddChild(columnName)
//                };
//            }

            {
                var literal = CreateNode("Literal");
                var groupOpen = CreateNode("GroupOpen").AddChild(CreateNode("GroupClose").AddChild(literal));
                var statement = CreateNode("Statement").AddChild(CreateNode("CompareOperator")
                    .AddChild(groupOpen).AddChild(literal));

                var columnName = CreateNode("ColumnName")
                    .AddChild(statement)
                    .AddChild(literal);

                yield return new TestCase("Multiple optional parts")
                {
                    Grammar = @"
                        $Something = Statement CompareOperator
                        $Something2 = GroupOpen GroupClose
                        $AllFilters = ColumnName ?$Something ?$Something2 Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = new RootNode().AddChild(columnName)
                };
            }

            // TODO: optional element before optional part
            // TODO: optional element as first in the part before optional part (e.g. $AllFilters = ?ColumnName ?$Something Literal)
            // TODO: optional element after optional part
            // TODO: optional element as last in the part after optional part (e.g. $AllFilters = ColumnName ?$Something ?Literal)

            {
                var literal = CreateNode("Literal");
                var columnName = CreateNode("ColumnName")
                    .AddChild(CreateNode("CompareOperator", true)
                        .AddChild(CreateNode("Statement", true)
                            .AddChild(literal)))
                    .AddChild(literal);

                yield return new TestCase("Multiple optional elements")
                {
                    Grammar = @"
                        $AllFilters = ColumnName ?CompareOperator ?Statement Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = new RootNode().AddChild(columnName)
                };
            }

            {
                var root = new RootNode();

                var groupOpen = CreateNode("GroupOpen", true);
                root.AddChild(groupOpen);

                var columnName = CreateNode("ColumnName")
                    .AddChild(CreateNode("CompareOperator").AddChild(CreateNode("Literal")));
                root.AddChild(columnName);
                groupOpen.AddChild(columnName);

                yield return new TestCase("Optional at the beginning of grammar")
                {
                    Grammar = @"
                        $AllFilters = ?GroupOpen ColumnName CompareOperator Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var columnName = CreateNode("ColumnName")
                    .AddChild(CreateNode("CompareOperator").AddChild(CreateNode("Literal", true)));
                root.AddChild(columnName);

                yield return new TestCase("Optional at the end of grammar")
                {
                    Grammar = @"
                        $AllFilters = ColumnName CompareOperator ?Literal
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupOpen = CreateNode("GroupOpen");
                var groupClose = CreateNode("GroupClose");

                var literal = CreateNode("Literal")
                    .AddChild(groupClose);
                var compareOperator = CreateNode("CompareOperator").AddChild(literal);
                var columnName = CreateNode("ColumnName", true)
                    .AddChild(compareOperator);

                groupOpen = groupOpen.AddChild(columnName).AddChild(compareOperator);

                root.AddChild(groupOpen);

                yield return new TestCase("Optional at the beginning of part")
                {
                    Grammar = @"
                        $Compare = ?ColumnName CompareOperator Literal
                        $AllFilters = GroupOpen $Compare GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupClose = CreateNode("GroupClose");

                var literal = CreateNode("Literal", true)
                    .AddChild(groupClose);
                var columnName = CreateNode("ColumnName")
                    .AddChild(CreateNode("CompareOperator")
                        .AddChild(literal)
                        .AddChild(groupClose));
                root.AddChild(columnName);

                yield return new TestCase("Optional at the end of part")
                {
                    Grammar = @"
                        $Compare = ColumnName CompareOperator ?Literal
                        $AllFilters = $Compare GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupOpen = CreateNode("GroupOpen");
                var columnName = CreateNode("ColumnName", true);
                var compOp = CreateNode("CompareOperator");
                var statement = CreateNode("Statement");
                var groupClose = CreateNode("GroupClose");

                groupOpen.AddChild(columnName);
                groupOpen.AddChild(compOp);
                columnName.AddChild(compOp);
                compOp.AddChild(statement);
                statement.AddChild(groupClose);

                root.AddChild(groupOpen);

                yield return new TestCase("Optional before part usage")
                {
                    Grammar = @"
                        $Compare = CompareOperator Statement
                        $AllFilters = GroupOpen ?ColumnName $Compare GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupOpen = CreateNode("GroupOpen");
                var columnName = CreateNode("ColumnName", true);
                var compOp = CreateNode("CompareOperator");
                var statement = CreateNode("Statement");
                var groupClose = CreateNode("GroupClose");

                groupOpen.AddChild(compOp);
                compOp.AddChild(statement);
                statement.AddChild(columnName);
                statement.AddChild(groupClose);
                columnName.AddChild(groupClose);

                root.AddChild(groupOpen);

                yield return new TestCase("Optional after part usage")
                {
                    Grammar = @"
                        $Compare = CompareOperator Statement
                        $AllFilters = GroupOpen $Compare ?ColumnName GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupClose = CreateNode("GroupClose");
                var literal = CreateNode("Literal", true).AddChild(groupClose);
                var compOp = CreateNode("CompareOperator").AddChild(literal).AddChild(groupClose);
                var statement = CreateNode("Statement").AddChild(literal).AddChild(groupClose);

                root.AddChild(compOp);
                root.AddChild(statement);

                yield return new TestCase("Optional after or condition")
                {
                    Grammar = @"
                        $AllFilters = CompareOperator|Statement ?Literal GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var compOp = CreateNode("CompareOperator");
                var statement = CreateNode("Statement");
                var literal = CreateNode("Literal", true).AddChild(compOp).AddChild(statement);
                var groupOpen = CreateNode("GroupOpen").AddChild(literal).AddChild(compOp).AddChild(statement);

                root.AddChild(groupOpen);

                yield return new TestCase("Or condition after optional")
                {
                    Grammar = @"
                        $AllFilters = GroupOpen ?Literal CompareOperator|Statement
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupClose = CreateNode("GroupClose");
                var @in = CreateNode("In").AddChild(groupClose);
                var compOp = CreateNode("CompareOperator").AddChild(groupClose);
                var columnName = CreateNode("ColumnName").AddChild(@in);
                var statement = CreateNode("Statement").AddChild(@in);
                var literal = CreateNode("Literal", true).AddChild(compOp).AddChild(columnName).AddChild(statement);
                var groupOpen = CreateNode("GroupOpen").AddChild(literal).AddChild(compOp).AddChild(columnName)
                    .AddChild(statement);

                root.AddChild(groupOpen);

                yield return new TestCase("Or condition with nested or after optional")
                {
                    Grammar = @"
                        $NestedOr = ColumnName|Statement In
                        $AllFilters = GroupOpen ?Literal CompareOperator|$NestedOr GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            {
                var root = new RootNode();

                var groupClose = CreateNode("GroupClose");

                var @in = CreateNode("In").AddChild(groupClose);
                var columnName = CreateNode("ColumnName").AddChild(@in);
                var statement = CreateNode("Statement").AddChild(@in);

                var @params = CreateNode("ParamsSeparator").AddChild(groupClose);
                var compareOperator = CreateNode("CompareOperator").AddChild(@params);
                var functionName = CreateNode("FunctionName").AddChild(@params);

                var literal = CreateNode("Literal", true).AddChild(columnName).AddChild(statement)
                    .AddChild(compareOperator).AddChild(functionName);
                var groupOpen = CreateNode("GroupOpen").AddChild(literal).AddChild(columnName).AddChild(statement)
                    .AddChild(compareOperator).AddChild(functionName);

                root.AddChild(groupOpen);

                yield return new TestCase("Or condition with multiple parts nested or after optional")
                {
                    Grammar = @"
                        $NestedOr = ColumnName|Statement In
                        $NestedOr2 = CompareOperator|FunctionName ParamsSeparator
                        $AllFilters = GroupOpen ?Literal $NestedOr|$NestedOr2 GroupClose
                        return $AllFilters
                    ",
                    ExpectedRoot = root
                };
            }

            // TODO: Optional at the end of part and then more optionals in upper part

            // TODO: optional in the end / at the beggining of or condition operand
            // TODO: multiple optional elements (parts/tokenizers)


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
//                        $Value = Literal
//
//                        $FunctionInvokation = FunctionName ParamsOpen $Params ParamsClose
//                        $Params = $Value ?$MoreParams
//                        $MoreParams = ParamsSeparator $Params
//
//                        return $FunctionInvokation
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
                        $Value = Literal|$FunctionInvokation

                        $FunctionInvokation = FunctionName ParamsOpen ?$Params ParamsClose
                        $Params = $Value

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

        private static TokenizerNode CreateNode(string name, bool isOptional = false)
        {
            return new TokenizerNode(new GrammarTokenizerUsage(name, isOptional, _tokenizersSource.GetTokenizer(name)));
        }

        private static LexerExecutionGraph CreateGraph(string grammar)
        {
            var reader = new LexerStringReader(grammar, 0);
            var parser = new LexerGenerator.ParserImpl(reader, _tokenizersSource);
            var root = parser.Parse();

            return LexerExecutionGraph.Build(root);
        }
    }
}