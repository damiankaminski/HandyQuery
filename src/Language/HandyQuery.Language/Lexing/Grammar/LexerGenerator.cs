using System.IO;
using System.Reflection;

namespace HandyQuery.Language.Lexing.Grammar
{
    internal sealed class LexerGenerator
    {
        /// <summary>
        /// Generates a new lexer which can be then reused to tokenize user queries.
        /// </summary>
        /// <remarks>Lexer is generated only once, so there is no need to avoid allocations.</remarks>
        public Lexer GenerateLexer()
        {
            var tokenizersSource = new TokenizersSource();
            var notFoundException = new QueryLanguageException("Grammar not found.");
            
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("HandyQuery.Language.Lexing.Grammar.Language.grammar"))
            using (var textStream = new StreamReader(stream ?? throw notFoundException))
            {
                var grammarText = textStream.ReadToEnd();
                var reader = new LexerStringReader(grammarText, 0);
                var parser = new GrammarParser(reader, tokenizersSource);
                var grammar = parser.Parse();

                return Lexer.Build(grammar);
            }
        }
    }
}