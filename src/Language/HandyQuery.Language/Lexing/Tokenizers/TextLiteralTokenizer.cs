using System;
using System.Collections.Generic;
using System.Text;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Lexing.Tokenizers
{
    [Tokenizer(typeof(KeywordTokenizer))]
    internal sealed class TextLiteralTokenizer : TokenizerBase
    {
        private static readonly Dictionary<char, char> EscapeSequences = new Dictionary<char, char>()
        {
            {'0', '\0'},
            {'a', '\a'},
            {'b', '\b'},
            {'f', '\f'},
            {'n', '\n'},
            {'r', '\r'},
            {'t', '\t'},
            {'v', '\v'},
        };

        public TextLiteralTokenizer(LanguageConfig languageConfig) : base(languageConfig)
        {
        }

        [HotPath]
        public override TokenizationResult Tokenize(ref LexerRuntimeInfo info)
        {
            ref var reader = ref info.Reader;

            var startPosition = reader.CaptureCurrentPosition();

            var identifier = LanguageConfig.Syntax.StringLiteralIdentifier;

            if (reader.CurrentChar != identifier)
            {
                // not '"' thus read only single word

                var singleWord = reader.ReadTillIvalidCharOrWhitespace(LanguageConfig.Syntax.ReservedChars);

                return TokenizationResult.Successful(
                    new TextLiteralToken(
                        startPosition.Value,
                        singleWord.Length,
                        new string(singleWord)));
            }

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

                if (isInEscapeContext)
                {
                    hasEscapeChar = true;
                    if (reader.MoveNext()) length++; // ignore escape char
                    else break;
                }
                else if (c == identifier)
                {
                    isProperlyClosed = true;
                    break;
                }

                if (c == '\n' || c == '\r') break;
            } while (reader.MoveNext());

            if (!isProperlyClosed)
            {
                // string not closed with '"'

                reader.MoveTo(startPosition);
                var word = reader.ReadTillEndOfWord(); // assumes it meant to be single word

                return TokenizationResult.PartiallySuccessful(
                    new TextLiteralToken(
                        startPosition.Value,
                        word.Length,
                        EvaluateTextLiteral(word.Slice(1), hasEscapeChar)
                    )
                );
            }

            var value = reader.Query.Slice(startPosition.Value, length);

            return TokenizationResult.Successful(
                new TextLiteralToken(
                    startPosition.Value,
                    value.Length,
                    EvaluateTextLiteral(value.Slice(1, value.Length - 2), hasEscapeChar)
                )
            );
        }

        private static string EvaluateTextLiteral(ReadOnlySpan<char> text, bool hasEscapeChar)
        {
            // TODO: try to optimize:
            // 1. string allocation is rather not avoidable, but maybe do it lazy? it might be not needed after all
            // 2. pool string builders?

            if (!hasEscapeChar) return new string(text);

            var sb = new StringBuilder(text.Length);

            var isInEscapeContext = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                isInEscapeContext = !isInEscapeContext && c == '\\';
                if (!isInEscapeContext)
                {
                    sb.Append(c);
                    continue;
                }

                if (text.Length > i + 1)
                {
                    // ignore escape initiator "\" and move on to escape char
                    i++;
                    c = text[i];

                    if (EscapeSequences.TryGetValue(c, out var escapeChar))
                    {
                        sb.Append(escapeChar);
                        continue;
                    }
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}