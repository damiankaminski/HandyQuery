using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Gramma;
using HandyQuery.Language.Lexing.Gramma.Structure;
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
            var expected = new LexerExecutionGraph(testCase.Root);

            var graph = CreateGraph(testCase.Gramma);

            graph.Equals(expected).Should().BeTrue();
        }

        private static IEnumerable<TestCase> GetTestCases()
        {
            yield return new TestCase()
            {
                Name = "Simple part",
                Gramma = @"
                    $AllFilters = ColumnName Statement
                    return $AllFilters
                ",
                Root = new Node(null).AddChild(
                    CreateNode("ColumnName").AddChild(
                        CreateNode("Statement")))
            };
        }

        public sealed class TestCase
        {
            public string Name { get; set; }
            public string Gramma { get; set; }
            internal Node Root { get; set; }

            public override string ToString()
            {
                return Name.Replace(' ', '_');
            }
        }

        private static Node CreateNode(string name, bool isOptional = false)
        {
            return new Node(CreateTokenizerUsage(name, isOptional));
        }

        private static GrammaTokenizerUsage CreateTokenizerUsage(string name, bool isOptional = false)
        {
            return new GrammaTokenizerUsage(name, isOptional, TokenizersSource.GetTokenizer(name));
        }

        private static LexerExecutionGraph CreateGraph(string gramma)
        {
            var reader = new LexerStringReader(gramma, 0);
            var parser = new LexerGenerator.ParserImpl(reader, TokenizersSource);
            var root = parser.Parse();

            return LexerExecutionGraph.Build(root);
        }
    }
}