using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class StatementTokenizer : KeywordTokenizerBase<StatementToken>
    {
        public StatementTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override StatementToken CreateToken(int startPosition, int length, Keyword keyword) 
            => new StatementToken(startPosition, length, keyword);

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