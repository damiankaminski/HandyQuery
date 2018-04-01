using System.Collections.Generic;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class CompareOperatorTokenizer : KeywordTokenizerBase<CompareOperatorToken>
    {
        [HotPath]
        public override CompareOperatorToken CreateToken(int startPosition, int length, Keyword keyword)
            => new CompareOperatorToken(startPosition, length, keyword);

        [HotPath]
        public override IEnumerable<Keyword> GetCandidatesForKeyword(in LexerRuntimeInfo info)
            => info.Config.Syntax.CompareOperators;

        [HotPath]
        public override Error OnNotFoundError(string word)
            => new Error($"\"{word}\" is not a compare operator.", ErrorId.CompareOperatorNotFound, word);
    }
}