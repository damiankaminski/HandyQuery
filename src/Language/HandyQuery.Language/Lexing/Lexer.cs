using System;
using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Lexing.Tokens;
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
        public LexerResult Tokenize(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException();

            var finalResult = new LexerResult();
            var reader = new LexerStringReader(query, 0);

            while (reader.IsEndOfQuery() == false)
            {
                reader.MoveBy(SkipWhitespaces(reader));

                var tokenFound = false;
                
                foreach (var tokenizer in _languageConfig.TokenizersSource.OrderedTokenizers)
                {
                    var runtimeInfo = new LexerRuntimeInfo(reader);
                    var result = tokenizer.Tokenize(ref runtimeInfo);
                    if (result.Success || result.IsPartiallySuccessful)
                    {
                        finalResult.Tokens.Add(result.Token);
                        reader.ForceMoveBy(result.Token.Length);
                        tokenFound = true;
                        break;
                    }
                }

                if (tokenFound == false)
                {
                    var startPosition = reader.CurrentPosition;
                    var word = reader.ReadTillEndOfWord();
                    var unknownToken = new UnknownToken(startPosition, word.Length);
                    finalResult.Tokens.Add(unknownToken);
                }
            }
            
            return finalResult;
        }

        private int SkipWhitespaces(LexerStringReader reader)
        {
            var whitespaceRuntimeInfo = new LexerRuntimeInfo(reader);
            var whitespaceResult = _whitespaceTokenizer.Tokenize(ref whitespaceRuntimeInfo);
            return whitespaceResult.Token?.Length ?? 0;
        }
    }
}