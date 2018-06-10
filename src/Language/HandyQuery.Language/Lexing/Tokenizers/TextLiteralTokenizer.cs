using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    // TODO: tests

    internal sealed class TextLiteralTokenizer : TokenizerBase
    {
        public TextLiteralTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            var startPosition = info.Reader.CaptureCurrentPosition();

            var identifier = LanguageConfig.Syntax.StringLiteralIdentifier;

            if (info.Reader.CurrentChar == identifier)
            {
                if (!info.Reader.MoveNext())
                {
                    return TokenizationResult.Failed();
                }

                // TODO: allow for escape via \
                var value = info.Reader.ReadWhile(identifier, (x, i) => x != i && x != '\n' && x != '\r');

                if (info.Reader.CurrentChar != identifier)
                {
                    // string not closed

                    info.Reader.MoveTo(startPosition);
                    var word = info.Reader.ReadTillEndOfWord(); // assumes it meant to be single word

                    return TokenizationResult.PartiallySuccessful(
                        new TextLiteralToken(
                            startPosition.Value,
                            word.Length,
                            new string(word.Slice(1))));
                }

                return TokenizationResult.Successful(
                    new TextLiteralToken(
                        startPosition.Value,
                        value.Length,
                        new string(value.Slice(1, value.Length - 2))));
            }

            var singleWord = info.Reader.ReadTillIvalidCharOrWhitespace(LanguageConfig.Syntax.ReservedChars);

            return TokenizationResult.Successful(
                new TextLiteralToken(
                    startPosition.Value,
                    singleWord.Length,
                    new string(singleWord)));
        }
    }
}