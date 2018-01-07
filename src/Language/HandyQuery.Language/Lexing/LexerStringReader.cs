using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Lexing
{
    // TODO: change to struct and reuse it all over the place
    // TODO: use Span instead of string API
    
    /// <summary>
    /// Provides methods to read a string using different scenarios, e.g. till the end of whitespaces, or till the first whitespace.
    /// </summary>
    internal sealed class LexerStringReader
    {
        public LexerStringReader(string query, int position)
        {
            if (position >= query.Length) throw new IndexOutOfRangeException();
            
            CurrentPosition = position;
            Query = query;
            QueryLength = query.Length;
        }

        private string Query { get; }
        private int QueryLength { get; }

        public int CurrentPosition { get; private set; }

        /// <summary>
        /// A number of chars read by 'ReadTill...' methods.
        /// </summary>
        public int ReadLength { get; private set; } // TODO: not sure if that's ever needed, length of returned string/span could be used instead

        public char CurrentChar => Query[CurrentPosition];

        /// <summary>
        /// Reads the <see cref="Query"/> till first non whitespace occurrence.
        /// </summary>
        public string ReadTillEndOfWhitespace()
        {
            return ReadWhile(char.IsWhiteSpace);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till first whitespace occurence.
        /// </summary>
        public string ReadTillEndOfWord()
        {
            return ReadWhile(x => char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till finds <see cref="x"/> words separated by whitespaces.
        /// </summary>
        public string ReadTillEndOfXWords(int x)
        {
            // TODO: fix heap alloc via closure
            var counter = 0;
            var isWithinWhitespaceContext = false;
            
            return ReadWhile(c =>
            {
                if (char.IsWhiteSpace(c))
                {
                    if (isWithinWhitespaceContext == false)
                    {
                        counter++;
                    }

                    isWithinWhitespaceContext = true;
                }
                else
                {
                    isWithinWhitespaceContext = false;
                }

                return counter < x;
            });
        }
        
        /// <summary>
        /// Reads the <see cref="Query"/> till end of number.
        /// </summary>
        public string ReadTillEndOfNumber(char seperator)
        {
            // TODO: fix heap alloc via closure
            return ReadWhile(x => char.IsDigit(x) || x == seperator);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till the first occurrance of invalid char.
        /// </summary>
        public string ReadTillIvalidChar(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            // TODO: fix heap alloc via closure
            return ReadWhile(x => invalidChars.Contains(x) == false);
        }

        /// <summary>
        /// Reads the <see cref="Query"/> till the first occurrance of invalid char.
        /// </summary>
        public string ReadTillIvalidCharOrWhitespace(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            // TODO: fix heap alloc via closure
            return ReadWhile(x => invalidChars.Contains(x) == false && char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads until new line character is found.
        /// </summary>
        public string ReadTillNewLine()
        {
            return ReadWhile(x => x != '\n' && x != '\r');
        }

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

        /// <summary>
        /// Determines whether <see cref="Query"/> in <see cref="CurrentPosition"/> starts with given value.
        /// </summary>
        public bool StartsWith(string value)
        {
            if (value == null) throw new ArgumentException(nameof(value));
            if (value == string.Empty) return true;
            
            var valueLength = value.Length;
            
            if (IsInRange(CurrentPosition + valueLength - 1) == false)
            {
                return false;
            }
            
            for (var i = 0; i < valueLength; i++)
            {
                var pos = CurrentPosition + i;
                
                if (Query[pos] != value[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks wheter <see cref="CurrentPosition"/> is in range of <see cref="Query"/>.
        /// </summary>
        public bool IsInRange() => IsInRange(CurrentPosition);

        /// <summary>
        /// Checks wheter given <see cref="position"/> is in range of <see cref="Query"/>.
        /// </summary>
        public bool IsInRange(int position) => QueryLength > position && position >= 0;
 
        /// <summary>
        /// Checks wheter everything has been already read.
        /// </summary>
        public bool IsEndOfQuery() => IsInRange(CurrentPosition + 1) == false;

        /// <summary>
        /// Moves to next char. Increases <see cref="ReadLength"/>.
        /// </summary>
        public bool MoveNext() => MoveBy(1);

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
            var queryLength = Query.Length;
            for (var i = CurrentPosition; i < queryLength; i++)
            {
                if (i >= queryLength) return false;

                if (Query[i] == '\n') return MoveBy(i - CurrentPosition + 1);

                if (Query[i] == '\r')
                {
                    if (MoveBy(i - CurrentPosition + 1) == false) return false;
                    if (CurrentChar == '\n') return MoveBy(1);
                    return true;
                }
            }

            return false;
        }

        public bool IsEndOfLine()
        {
            return CurrentChar == '\r' || CurrentChar == '\n';
        }

        /// <summary>
        /// Reads the query while predicate returns true.
        /// </summary>
        public string ReadWhile(Func<char, bool> predicate)
        {
            if (IsInRange(CurrentPosition) == false)
            {
                return "";
            }

            var length = 0;
            var startIndex = CurrentPosition;

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
    }
}