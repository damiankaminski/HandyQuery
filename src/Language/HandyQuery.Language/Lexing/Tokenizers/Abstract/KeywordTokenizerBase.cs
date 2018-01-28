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
            // TODO:
            
//            var reader = info.Reader;
//            var startPosition = reader.CurrentPosition;
//            var text = reader.ReadTillEndOfKeyword(
//                GetCandidatesForKeyword(info), 
//                info.CurrentCulture, 
//                info.Config.SyntaxInfo);
//
//            var keyword = info.CurrentCulture.GetKeyword(text);
//            var token = CreateToken(startPosition, reader.ReadLength, keyword);
//
//            if (token.Keyword == null)
//            {
//                reader.RestoreCurrentPosition();
//                var word = reader.ReadTillEndOfWord();
//                return TokenizationResult.Failed(OnNotFoundError(word));
//            }
//
//            return TokenizationResult.Successful(token);
            return TokenizationResult.Successful(null);
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
        
        // TODO: move outside of LexerStringReader? maybe as an extension method?
        // TODO: avoid heap allocations
        // TODO: test
//        /// <summary>
//        /// Reads the <see cref="Query"/> till the end of keyword.
//        /// </summary>
//        public string ReadTillEndOfKeyword(IEnumerable<Keyword> keywords, ICultureConfig culture, SyntaxInfo syntax)
//        {
//            var result = string.Empty;
//            var keywordTexts = keywords.Select(culture.GetKeywordText).OrderByDescending(x => x.Length).ToArray();
//            var caseSensitive = syntax.Config.KeywordCaseSensitive;
//            var reservedChars = syntax.ReservedChars;
//
//            foreach (var keyword in keywordTexts)
//            {
//                var keywordName = caseSensitive== false ? keyword.ToLowerInvariant() : keyword;
//
//                var prevIndex = CurrentPosition - 1;
//                var nextIndex = CurrentPosition + keywordName.Length;
//                var prevChar = prevIndex < 0 ? null as char? : Query[prevIndex];
//                var nextChar = IsInRange(nextIndex) == false ? null as char? : Query[nextIndex];
//
//                // keyword out of range of provided query
//                if (IsInRange(nextIndex - 1) == false)
//                {
//                    continue;
//                }
//
//                // keyword not found
//                var queryPart = Query.Substring(CurrentPosition, keywordName.Length);
//                queryPart = caseSensitive == false ? queryPart.ToLowerInvariant() : queryPart;
//                if (keywordName != queryPart)
//                {
//                    continue;
//                }
//
//                // keyword starts with letter or digit and previous char isn't special character or whitespace
//                // e.g. Namestarts with
//                //          ^
//                if (char.IsLetterOrDigit(keywordName.First()) && CurrentPosition > 0
//                    && char.IsWhiteSpace(prevChar.Value) == false
//                    && reservedChars.Contains(prevChar.Value) == false)
//                {
//                    continue;
//                }
//
//                // keyword ends with letter or digit and special character or whitespace after is missing
//                // e.g. Name starts withDamian
//                //           ^
//                if (char.IsLetterOrDigit(keywordName.Last()) && IsInRange(nextIndex)
//                    && char.IsWhiteSpace(nextChar.Value) == false
//                    && reservedChars.Contains(nextChar.Value) == false)
//                {
//                    continue;
//                }
//
//                result = keywordName;
//                break;
//            }
//
//            ReadLength = result.Length;
//            return result;
//        }
    }
}