using System;
using System.Collections.Generic;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    internal abstract class KeywordTokenizerBase<TKeywordToken> : ITokenizer
        where TKeywordToken : KeywordTokenBase
    {
        public TokenizationResult Tokenize(LexerRuntimeInfo info)
        {
            var reader = info.Reader;
            var startPosition = reader.CurrentPosition;
            var text = reader.ReadTillEndOfKeyword(
                GetCandidatesForKeyword(info), 
                info.CurrentCulture, 
                info.Config.SyntaxInfo);

            var keyword = info.CurrentCulture.GetKeyword(text);
            var token = CreateToken(startPosition, reader.ReadLength, keyword);

            if (token.Keyword == null)
            {
                reader.RestoreCurrentPosition();
                var word = reader.ReadTillEndOfWord();
                return TokenizationResult.Failed(OnNotFoundError(word));
            }

            return TokenizationResult.Successful(token);
        }

        /// <summary>
        /// Should create an instance of <see cref="TKeywordToken"/> using provided parameters.
        /// </summary>
        public abstract TKeywordToken CreateToken(int startPosition, int length, Keyword keyword);

        /// <summary>
        /// Should provide possible keywords.
        /// </summary>
        public abstract IEnumerable<Keyword> GetCandidatesForKeyword(LexerRuntimeInfo info);

        /// <summary>
        /// Allows to add an error in case if keyword is not found.
        /// </summary>
        public abstract Error OnNotFoundError(string word);
    }
}