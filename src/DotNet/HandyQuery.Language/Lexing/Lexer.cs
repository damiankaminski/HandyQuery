using System;
using System.Collections.Generic;
using System.Globalization;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Graph;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing
{
    internal sealed class Lexer
    {
        internal readonly LexerExecutionGraph ExecutionGraph;

        private readonly Stack<Stack<IGrammarBodyItem>> _state = new Stack<Stack<IGrammarBodyItem>>(); // TODO: move it inner class to make Execute thread safe

        private Lexer(LexerExecutionGraph executionGraph)
        {
            ExecutionGraph = executionGraph;
        }

        public static Lexer Build(GrammarReturn root)
        {
            return new Lexer(LexerExecutionGraph.Build(root));
        }
        
        public LexerResult Execute(string query, ILanguageInternalConfig languageConfig, CultureInfo cultureInfo, LexerConfig config = null)
        {
            config = config ?? LexerConfig.Default;
            var finalResult = new LexerResult();
            var reader = new LexerStringReader(query, 0); // TODO: pool
            var runtimeInfo = new LexerRuntimeInfo(reader, languageConfig, cultureInfo); // TODO: pool

//            var current = _root.Body.FirstOrDefault();
//
//            if (current == null)
//            {
//                return finalResult;
//            }
//
//            Process(current, reader, finalResult, runtimeInfo);

            return finalResult;
        }

        private static void Process(IGrammarElement current, LexerStringReader reader, LexerResult finalResult, LexerRuntimeInfo runtimeInfo)
        {
            if (reader.IsEndOfQuery())
            {
                return;
            }

            var position = reader.CurrentPosition;
            var whitespace = reader.ReadTillEndOfWhitespace();
            if (whitespace.Length > 0)
            {
                finalResult.Tokens.Add(new WhitespaceToken(position, whitespace.Length));
            }

            if (current.Type == GrammarElementType.PartUsage)
            {
                var part = current.As<GrammarPartUsage>();
                // TODO: part.IsOptional
                foreach (var element in part.Impl.Body)
                {
                    // TODO: what if failed? restorable via stack
                    Process(element, reader, finalResult, runtimeInfo);
                }
            }

            if (current.Type == GrammarElementType.TokenizerUsage)
            {
                var tokenizerUsage = (GrammarTokenizerUsage)current;
                // TODO: tokenizer.IsOptional
                reader.CaptureCurrentPosition();
                var result = tokenizerUsage.Impl.Tokenize(runtimeInfo);
                reader.RestoreCurrentPosition();
                if (result.Success)
                {
                    reader.MoveBy(result.Token.Length);
                    finalResult.Tokens.Add(result.Token);
                    // TODO: move to next gramma element
                }

                // TODO: save errors if all tokenizers fails
            }

            throw new InvalidOperationException(); // TODO: change
        }
    }
}