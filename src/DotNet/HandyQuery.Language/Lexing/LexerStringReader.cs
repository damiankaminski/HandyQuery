using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Lexing
{
    // TODO: Lamba usage allocates on the heap. Get rid of it.

    /// <summary>
    /// Provides methods to read a string using different scenarios, e.g. till the end of whitespaces, or till the first whitespace.
    /// Not thread safe.
    /// </summary>
    internal sealed class LexerStringReader
    {
        public LexerStringReader(string query, int currentPosition)
        {
            CurrentPosition = currentPosition;
            Query = query;
            QueryLength = query.Length;
        }

        public string Query { get; }
        private int QueryLength { get; }

        public int CurrentPosition { get; private set; }
        private int CapturedCurrentPosition { get; set; }

        /// <summary>
        /// If true then length is being reset on each `ReadSomething` method invokation.
        /// </summary>
        public bool ResetLengthOnEachRead { get; set; } = true;

        /// <summary>
        /// A number of chars read by 'ReadTill...' methods.
        /// </summary>
        public int ReadLength { get; private set; }

        public char CurrentChar => Query[CurrentPosition];

        /// <summary>
        /// Reads the <see cref="Query"/> till first invokation of <see cref="predicate"/> with `true` as a result.
        /// </summary>
        public string ReadTill(Func<char, bool> predicate)
        {
            return ReadWhile(c => !predicate(c));
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till first non whitespace occurrence.
        /// </summary>
        public string ReadTillEndOfWhitespace()
        {
            return ReadWhile(Char.IsWhiteSpace);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till first whitespace occurence.
        /// </summary>
        public string ReadTillEndOfWord()
        {
            return ReadWhile(x => Char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till end of number.
        /// </summary>
        public string ReadTillEndOfNumber(char seperator)
        {
            return ReadWhile(x => Char.IsDigit(x) || x == seperator);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till <see cref="x"/> whitespaces.
        /// </summary>
        public string ReadTillEndOfXWords(int x)
        {
            var counter = 0;

            return ReadWhile(c =>
            {
                if (char.IsWhiteSpace(c))
                {
                    counter++;
                }

                return counter < x;
            });
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till the first occurrance of invalid char.
        /// </summary>
        public string ReadTillIvalidChar(IEnumerable<char> invalidChars)
        {
            return ReadWhile(x => invalidChars.Contains(x) == false);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till the first occurrance of invalid char.
        /// </summary>
        public string ReadTillIvalidCharOrWhitespace(IEnumerable<char> invalidChars)
        {
            return ReadWhile(x => invalidChars.Contains(x) == false && char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads until new line character is found.
        /// </summary>
        public string ReadTillNewLine()
        {
            return ReadWhile(x => x != '\n' && x != '\r');
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till the end of keyword.
        /// </summary>
        public string ReadTillEndOfKeyword(IEnumerable<Keyword> keywords, ICultureConfig culture, SyntaxInfo syntax)
        {
            var result = string.Empty;
            var keywordTexts = keywords.Select(culture.GetKeywordText).OrderByDescending(x => x.Length).ToArray();
            var caseSensitive = syntax.Config.KeywordCaseSensitive;
            var reservedChars = syntax.ReservedChars;

            foreach (var keyword in keywordTexts)
            {
                var keywordName = caseSensitive== false ? keyword.ToLowerInvariant() : keyword;

                var prevIndex = CurrentPosition - 1;
                var nextIndex = CurrentPosition + keywordName.Length;
                var prevChar = prevIndex < 0 ? null as char? : Query[prevIndex];
                var nextChar = IsInRange(nextIndex) == false ? null as char? : Query[nextIndex];

                // keyword out of range of provided query
                if (IsInRange(nextIndex - 1) == false)
                {
                    continue;
                }

                // keyword not found
                var queryPart = Query.Substring(CurrentPosition, keywordName.Length);
                queryPart = caseSensitive == false ? queryPart.ToLowerInvariant() : queryPart;
                if (keywordName != queryPart)
                {
                    continue;
                }

                // keyword starts with letter or digit and previous char isn't special character or whitespace
                // e.g. Namestarts with
                //          ^
                if (char.IsLetterOrDigit(keywordName.First()) && CurrentPosition > 0
                    && char.IsWhiteSpace(prevChar.Value) == false
                    && reservedChars.Contains(prevChar.Value) == false)
                {
                    continue;
                }

                // keyword ends with letter or digit and special character or whitespace after is missing
                // e.g. Name starts withDamian
                //           ^
                if (char.IsLetterOrDigit(keywordName.Last()) && IsInRange(nextIndex)
                    && char.IsWhiteSpace(nextChar.Value) == false
                    && reservedChars.Contains(nextChar.Value) == false)
                {
                    continue;
                }

                result = keywordName;
                break;
            }

            ReadLength = result.Length;
            return result;
        }

        /// <summary>
        /// Determines whether <see cref="Query"/> in <see cref="CurrentPosition"/> starts with provided value.
        /// </summary>
        public bool StartsWith(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (Query[CurrentPosition + i] != value[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks wheter <see cref="CurrentPosition"/> is in range of <see cref="Query"/>.
        /// </summary>
        public bool IsInRange()
        {
            return IsInRange(CurrentPosition);
        }

        /// <summary>
        /// Checks wheter given <see cref="position"/> is in range of <see cref="Query"/>.
        /// </summary>
        public bool IsInRange(int position)
        {
            return QueryLength > position;
        }

        /// <summary>
        /// Checks wheter everything has been already read.
        /// </summary>
        public bool IsEndOfQuery() => IsInRange(CurrentPosition + 1) == false;

        /// <summary>
        /// Moves to next char. Increases <see cref="ReadLength"/>.
        /// </summary>
        public bool MoveNext()
        {
            return MoveBy(1);
        }

        /// <summary>
        /// Moves <see cref="CurrentPosition"/> and <see cref="ReadLength"/> by <see cref="x"/>.
        /// </summary>
        public bool MoveBy(int x)
        {
            var newPosition = CurrentPosition + x;

            if (IsInRange(newPosition) == false)
            {
                return false;
            }

            CurrentPosition = newPosition;
            return true;
        }

        public bool MoveToNextLine()
        {
            var index = Query.IndexOf('\r', CurrentPosition);
            if (index == -1)
            {
                index = Query.IndexOf('\n', CurrentPosition);
                return index != -1 && MoveBy(index - CurrentPosition + 1);
            }
            if (MoveBy(index - CurrentPosition + 1) == false) return false;
            if (CurrentChar == '\n') return MoveBy(1);
            return true;
        }

        public bool IsNewLine()
        {
            return CurrentChar == '\r' || CurrentChar == '\n';
        }

        /// <summary>
        /// Reads query while predicate returns true.
        /// </summary>
        private string ReadWhile(Func<char, bool> predicate)
        {
            if (ResetLengthOnEachRead)
            {
                ReadLength = 0;
            }

            var length = 0;
            var startIndex = CurrentPosition;

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop - CurrentChar is changed by MoveNext method
            while (predicate(CurrentChar))
            {
                length++;

                if (MoveNext() == false)
                {
                    break;
                }
            }

            ReadLength += length;

            return Query.Substring(startIndex, length);
        }

        /// <summary>
        /// Resets read length to 0.
        /// </summary>
        public void ResetReadLength()
        {
            ReadLength = 0;
        }

        /// <summary>
        /// Captures current position and allows to restore it using <see cref="RestoreCurrentPosition"/> method.
        /// </summary>
        public void CaptureCurrentPosition()
        {
            CapturedCurrentPosition = CurrentPosition;
        }

        /// <summary>
        /// Restores current position to last captured by <see cref="CaptureCurrentPosition"/> method.
        /// </summary>
        public void RestoreCurrentPosition()
        {
            CurrentPosition = CapturedCurrentPosition;
        }
    }
}