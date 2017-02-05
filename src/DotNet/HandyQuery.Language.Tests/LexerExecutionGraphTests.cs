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
            var expected = new LexerExecutionGraph(testCase.Root);

            var graph = CreateGraph(testCase.Grammar);

            graph.Equals(expected).Should().BeTrue();
        }

        private static IEnumerable<TestCase> GetTestCases()
        {
            yield return new TestCase()
            {
                Name = "Simple part",
                Grammar = @"
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
            public string Grammar { get; set; }
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