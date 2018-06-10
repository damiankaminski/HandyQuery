using System.IO;
using System.Reflection;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;

namespace HandyQuery.Language.Parsing.Grammar
{
    internal sealed class ParserGenerator
    {
        /// <summary>
        /// Generates a new parser which can be then reused to parse user queries.
        /// </summary>
        /// <remarks>Parser is generated only once, so there is no need to avoid allocations.</remarks>
        public Parser GenerateParser(LanguageConfig languageConfig)
        {
            var notFoundException = new QueryLanguageException("Grammar not found.");
            
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("HandyQuery.Language.Parsing.Grammar.Language.grammar"))
            using (var textStream = new StreamReader(stream ?? throw notFoundException))
            {
                var grammarText = textStream.ReadToEnd();
                var parser = new GrammarParser(grammarText, null);
                var grammar = parser.Parse();

                return null; // TODO: create parser
            }
        }
    }
}