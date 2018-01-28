using System;
using System.Collections.Generic;
using System.Data;
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

        private Lexer(LexerExecutionGraph executionGraph)
        {
            ExecutionGraph = executionGraph;
        }

        public static Lexer Build(Grammar.Grammar grammar)
        {
            return new Lexer(LexerExecutionGraph.Build(grammar));
        }
        
        [PerformanceCritical]
        public LexerResult Tokenize(string query, ILanguageInternalConfig languageConfig, CultureInfo cultureInfo, LexerConfig config = null)
        {
            if(string.IsNullOrWhiteSpace(query)) throw new ArgumentException();
            
            //config = config ?? LexerConfig.Default;
            var finalResult = new LexerResult();
            //var reader = new LexerStringReader(query, 0); // TODO: pass by ref
            //var runtimeInfo = new LexerRuntimeInfo(reader, languageConfig, cultureInfo); // TODO: struct

            // TODO: implement
            
            return finalResult;
        }
    }
}