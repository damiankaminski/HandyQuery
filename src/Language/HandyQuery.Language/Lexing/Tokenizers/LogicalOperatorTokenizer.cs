using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class LogicalOperatorTokenizer : KeywordTokenizerBase<LogicalOperatorToken>
    {
        public LogicalOperatorTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override LogicalOperatorToken CreateToken(int startPosition, int length, Keyword keyword) 
            => new LogicalOperatorToken(startPosition, length, keyword);


        [HotPath]
        public override Error OnNotFoundError(string word, int position)
        {
            return new Error(
                $"\"{word}\" is not a logical operator.", 
                ErrorId.LogicalOperatorNotFound,
                new Error.RangeInfo(position, word.Length),
                word);
        }
    }
}