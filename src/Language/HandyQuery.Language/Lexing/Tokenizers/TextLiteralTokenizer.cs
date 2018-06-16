using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    internal sealed class TextLiteralTokenizer : TokenizerBase
    {
        public TextLiteralTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            ref var reader = ref info.Reader;
            
            var startPosition = reader.CaptureCurrentPosition();

            var identifier = LanguageConfig.Syntax.StringLiteralIdentifier;
            
            if (reader.CurrentChar == identifier)
            {
                // starts with '"'
                
                if (!reader.MoveNext())
                {
                    return TokenizationResult.Failed();
                }
                
                var length = 1;
                var isProperlyClosed = false; // indicates whether string is closed with '"'
                var isInEscapeContext = false;
                var hasEscapeChar = false;
                
                do
                {   
                    length++;
                    
                    var c = reader.CurrentChar;

                    isInEscapeContext = !isInEscapeContext && c == '\\';

                    if (isInEscapeContext) hasEscapeChar = true;
                    if (isInEscapeContext && !reader.MoveNext()) break;

                    if (c == identifier && !isInEscapeContext)
                    {
                        isProperlyClosed = true;
                        break;
                    }

                    if (c == '\n' || c == '\r') break;
                } while(reader.MoveNext());

                if (!isProperlyClosed)
                {
                    // string not closed with "

                    reader.MoveTo(startPosition);
                    var word = reader.ReadTillEndOfWord(); // assumes it meant to be single word

                    return TokenizationResult.PartiallySuccessful(
                        new TextLiteralToken(
                            startPosition.Value,
                            word.Length,
                            new string(word.Slice(1))));
                }

                // TODO: rewrite value if it has escape char (needs to be removed)
                
                var value = reader.Query.Slice(startPosition.Value, length);
                
                return TokenizationResult.Successful(
                    new TextLiteralToken(
                        startPosition.Value,
                        value.Length,
                        new string(value.Slice(1, value.Length - 2))));
            }

            var singleWord = reader.ReadTillIvalidCharOrWhitespace(LanguageConfig.Syntax.ReservedChars);

            return TokenizationResult.Successful(
                new TextLiteralToken(
                    startPosition.Value,
                    singleWord.Length,
                    new string(singleWord)));
        }
    }
}