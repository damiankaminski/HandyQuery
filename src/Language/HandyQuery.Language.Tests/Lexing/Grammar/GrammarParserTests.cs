using System;
using System.IO;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Grammar
{
    public class GrammarParserTests
    {
        private static readonly TokenizersSource TokenizersSource 
            = new TokenizersSource(HandyQueryLanguage.Configure<Person>().Build());

        [Test]
        public void Should_detect_obvious_infinite_recursions()
        {
            var expectedMessage = "Infinite recursion detected in <value> non-terminal. " +
                                  "Use '|' to create escape path.";
            Action action1 = () =>
            {
                Parse(@"
                        <value> : <value>
                        return <value>
                    ");
            };
            
            Action action2 = () =>
            {
                Parse(@"
                        <value> : <value> | <value>
                        return <value>
                    ");
            };
            
            action1.Should().ThrowExactly<GrammarParserException>().WithMessage(expectedMessage);
            action2.Should().ThrowExactly<GrammarParserException>().WithMessage(expectedMessage);
        }

        [Test]
        public void Should_work_with_real_grammar()
        {
            using (var stream = typeof(GrammarParser).Assembly
                .GetManifestResourceStream("HandyQuery.Language.Lexing.Grammar.Language.grammar"))
            using (var textStream = new StreamReader(stream))
            {
                var grammarText = textStream.ReadToEnd();
                var parser = new GrammarParser(grammarText, TokenizersSource);
                var grammar = parser.Parse();

                grammar.Should().NotBeNull();
            }
        }
        
        private static Language.Lexing.Grammar.Grammar Parse(string grammarText)
        {
            var parser = new GrammarParser(grammarText, TokenizersSource);
            return parser.Parse();
        }
    }
}