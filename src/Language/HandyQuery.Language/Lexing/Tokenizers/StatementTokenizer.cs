using System.Collections.Generic;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [PerformanceCritical]
    internal sealed class StatementTokenizer : KeywordTokenizerBase<StatementToken>
    {
        public override StatementToken CreateToken(int startPosition, int length, Keyword keyword) 
            => new StatementToken(startPosition, length, keyword);

        public override IEnumerable<Keyword> GetCandidatesForKeyword(in LexerRuntimeInfo info) 
            => info.Config.Syntax.Statements;

        public override Error OnNotFoundError(string word)
            => new Error($"\"{word}\" is not a statement.", ErrorId.StatementNotFound, word);
    }
}