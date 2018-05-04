using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class CompareOperatorTokenizer : KeywordTokenizerBase<CompareOperatorToken>
    {
        public CompareOperatorTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }
        
        [HotPath]
        public override CompareOperatorToken CreateToken(int startPosition, int length, Keyword keyword)
            => new CompareOperatorToken(startPosition, length, keyword);

        [HotPath]
        public override Error OnNotFoundError(string word, int position)
        {
            return new Error(
                $"\"{word}\" is not a compare operator.", 
                ErrorId.CompareOperatorNotFound,
                new Error.RangeInfo(position, word.Length),
                word);
        }
    }
}