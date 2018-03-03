using System;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Grammar
{
    public class GrammarParserTests
    {
        private static readonly TokenizersSource TokenizersSource = new TokenizersSource();

        [Test]
        public void ShouldDetectObviousInfiniteRecursions()
        {
            var expectedMessage = "Infinite recursion detected in <value> non-terminal. " +
                                  "Use '|' to create escape path.";
            Action action1 = () =>
            {
                Parse(@"
                        <value> ::= <value>
                        return <value>
                    ");
            };
            
            Action action2 = () =>
            {
                Parse(@"
                        <value> ::= <value> | <value>
                        return <value>
                    ");
            };
            
            action1.Should().ThrowExactly<GrammarParserException>().WithMessage(expectedMessage);
            action2.Should().ThrowExactly<GrammarParserException>().WithMessage(expectedMessage);
        }

        private static Language.Lexing.Grammar.Grammar Parse(string grammarText)
        {
            var parser = new GrammarParser(grammarText, TokenizersSource);
            return parser.Parse();
        }
    }
}