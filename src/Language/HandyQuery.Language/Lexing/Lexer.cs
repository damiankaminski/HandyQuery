using System;
using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Parsing.StateTransition.Nodes;

namespace HandyQuery.Language.Lexing
{
    internal sealed class Lexer
    {
        private readonly LanguageConfig _languageConfig;
        private readonly WhitespaceTokenizer _whitespaceTokenizer;
        
        private Lexer(LanguageConfig languageConfig)
        {
            _languageConfig = languageConfig;
            _whitespaceTokenizer =  new WhitespaceTokenizer(languageConfig);
        }

        public static Lexer Build(LanguageConfig languageConfig)
        {
            return new Lexer(languageConfig);
        }

        [HotPath]
        public LexerResult Tokenize(string query, LexerConfig config = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException();

            config = config ?? LexerConfig.Default;
            
            var tokenizersSource = new TokenizersSource(_languageConfig);
            var finalResult = new LexerResult();
            var reader = new LexerStringReader(query, 0);
            var restorableReader = new LexerStringReader.Restorable();
            var runtimeInfo = new LexerRuntimeInfo(reader); // TODO: ref reader?

            // TODO

            return finalResult;
        }
    }
}