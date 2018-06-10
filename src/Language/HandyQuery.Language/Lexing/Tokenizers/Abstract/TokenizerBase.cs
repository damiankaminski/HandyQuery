using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing.Tokenizers.Abstract
{
    // TODO: maybe tokenizers should not return string errors, instead only error id and subject (>,< etc, via span)?
    // TODO: then error list could be grouped (by tokenizer type?) and error could be evaluate when needed (via `string Tokenizer.EvaluateError(IEnumerable<Error> errors)`?)
    
    internal abstract class TokenizerBase : ITokenizer
    {
        protected readonly LanguageConfig LanguageConfig;

        protected TokenizerBase(LanguageConfig languageConfig)
        {
            LanguageConfig = languageConfig;
        }

        [HotPath]
        public abstract TokenizationResult Tokenize(ref LexerRuntimeInfo info);

        /// <summary>
        /// Makes sure that there is a special character right after token if it ends with letter or digit.
        /// Changes result to failed if this condition is not met.
        /// </summary>
        /// <example>
        /// `Name starts with` -> good
        /// `"Name"starts with` -> good
        /// `Namestarts with` -> bad
        /// `Name5starts with` -> bad
        /// `Age >= 5` -> good
        /// `Age>=5` -> good
        /// </example>
        [HotPath]
        protected TokenizationResult EnsureTrailingSpecialChar(ref LexerRuntimeInfo info, TokenizationResult result)
        {
            if (result.Success == false) return result;

            ref var reader = ref info.Reader;
            var token = result.Token;

            var nextPosition = token.StartPosition + token.Length;
            if (reader.IsInRange(nextPosition) == false) return result;

            var lastPosition = token.StartPosition + token.Length - 1;
            if (char.IsLetterOrDigit(reader.Query[nextPosition]) && // next is letter or digit
                char.IsLetterOrDigit(reader.Query[lastPosition])) // last is letter or digit
            {
                // letter or digit right next to each other, between tokens, detected
                return TokenizationResult.Failed();
            }

            return result;
        }
    }
}