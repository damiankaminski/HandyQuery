using System.Collections.Generic;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class StatementTokenizer : KeywordTokenizerBase<StatementToken>
    {
        [HotPath]
        public override StatementToken CreateToken(int startPosition, int length, Keyword keyword) 
            => new StatementToken(startPosition, length, keyword);

        [HotPath]
        public override IEnumerable<Keyword> GetCandidatesForKeyword(in LexerRuntimeInfo info) 
            => info.Config.Syntax.Statements;

        [HotPath]
        public override Error OnNotFoundError(string word, int position)
        {
            return new Error(
                $"\"{word}\" is not a statement.", 
                ErrorId.StatementNotFound,
                new Error.RangeInfo(position, word.Length),
                word);
        }
    }
}