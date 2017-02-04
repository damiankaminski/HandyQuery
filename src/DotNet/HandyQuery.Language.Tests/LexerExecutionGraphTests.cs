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
        private static TokenizersSource _tokenizersSource;

        public LexerExecutionGraphTests()
        {
            _tokenizersSource = new TokenizersSource();
        }

        [Test]
        public void ShouldParsePartBody()
        {
            var graph = CreateGraph(@"
                $AllFilters = ColumnName Statement
                return $AllFilters
            ");

            var root = new Node(null).AddChild(
                new Node(CreateTokenizerUsage("ColumnName")).AddChild(
                    new Node(CreateTokenizerUsage("Statement"))));

            var expected = new LexerExecutionGraph(root);
            graph.Equals(expected).Should().BeTrue();
        }

        private GrammaTokenizerUsage CreateTokenizerUsage(string name, bool isOptional = false)
        {
            return new GrammaTokenizerUsage(name, isOptional, _tokenizersSource.GetTokenizer(name));
        }

        private GrammaPartUsage CreatePartUsage(GrammaPart part, bool isOptional = false)
        {
            return new GrammaPartUsage(part.Name, isOptional, part);
        }

        private GrammaPart CreatePart(string name, params IGrammaBodyItem[] bodyItems)
        {
            var body = new GrammaPartBody();
            body.AddRange(bodyItems);

            return new GrammaPart(name) {Body = body};
        }

        private static LexerExecutionGraph CreateGraph(string gramma)
        {
            var reader = new LexerStringReader(gramma, 0);
            var parser = new LexerGenerator.ParserImpl(reader, _tokenizersSource);
            var root = parser.Parse();

            return LexerExecutionGraph.Build(root);
        }
    }
}