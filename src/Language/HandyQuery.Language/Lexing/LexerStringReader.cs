using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing
{
    // TODO: rename to NoAllocStringReader?
    
    /// <summary>
    /// Provides methods to read a string using different scenarios, e.g. till the end of whitespaces, or till the first whitespace.
    /// </summary>
    internal ref struct LexerStringReader
    {
        public LexerStringReader(string query, int position)
        {
            if (position >= query.Length) throw new IndexOutOfRangeException();
            
            CurrentPosition = position;
            _query = query.AsReadOnlySpan();
            _queryLength = query.Length;
        }

        private readonly ReadOnlySpan<char> _query;
        private readonly int _queryLength;

        public int CurrentPosition { get; private set; }

        public char CurrentChar => _query[CurrentPosition];

        /// <summary>
        /// Reads the query till first non whitespace occurrence.
        /// </summary>
        public ReadOnlySpan<char> ReadTillEndOfWhitespace()
        {
            return ReadWhile(char.IsWhiteSpace);
        }

        /// <summary>
        /// Reads the query till first whitespace occurence.
        /// </summary>
        public ReadOnlySpan<char> ReadTillEndOfWord()
        {
            return ReadWhile(x => char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads the query till finds <see cref="x"/> words separated by whitespaces.
        /// </summary>
        public ReadOnlySpan<char> ReadTillEndOfXWords(int x)
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
        /// Reads the query till end of number.
        /// </summary>
        public ReadOnlySpan<char> ReadTillEndOfNumber(char seperator)
        {
            return ReadWhile(seperator, (sep, x) => char.IsDigit(x) || x == sep);
        }

        /// <summary>
        /// Reads the query till the first occurrance of invalid char.
        /// </summary>
        public ReadOnlySpan<char> ReadTillIvalidChar(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            return ReadWhile(invalidChars, (chars, x) => chars.Contains(x) == false);
        }

        /// <summary>
        /// Reads the query till the first occurrance of invalid char.
        /// </summary>
        public ReadOnlySpan<char> ReadTillIvalidCharOrWhitespace(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            return ReadWhile(invalidChars, (chars, x) => chars.Contains(x) == false && char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads until new line character is found.
        /// </summary>
        public ReadOnlySpan<char> ReadTillNewLine()
        {
            return ReadWhile(x => x != '\n' && x != '\r');
        }

        /// <summary>
        /// Determines whether query in <see cref="CurrentPosition"/> starts with given value.
        /// </summary>
        public bool StartsWith(ReadOnlySpan<char> value)
        {
            if (value.Length == 0) return true;
            
            var valueLength = value.Length;
            
            if (IsInRange(CurrentPosition + valueLength - 1) == false)
            {
                return false;
            }
            
            for (var i = 0; i < valueLength; i++)
            {
                var pos = CurrentPosition + i;
                
                if (_query[pos] != value[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks wheter <see cref="CurrentPosition"/> is in range of query.
        /// </summary>
        public bool IsInRange() => IsInRange(CurrentPosition);

        /// <summary>
        /// Checks wheter given <see cref="position"/> is in range of query.
        /// </summary>
        public bool IsInRange(int position) => _queryLength > position && position >= 0;
 
        /// <summary>
        /// Checks wheter everything has been already read.
        /// </summary>
        public bool IsEndOfQuery() => IsInRange(CurrentPosition + 1) == false;

        /// <summary>
        /// Moves to next char.
        /// </summary>
        public bool MoveNext() => MoveBy(1);

        /// <summary>
        /// Moves <see cref="CurrentPosition"/> by <see cref="x"/>.
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
            var queryLength = _query.Length;
            for (var i = CurrentPosition; i < queryLength; i++)
            {
                if (i >= queryLength) return false;

                if (_query[i] == '\n') return MoveBy(i - CurrentPosition + 1);

                if (_query[i] == '\r')
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
        public ReadOnlySpan<char> ReadWhile(Func<char, bool> predicate)
        {
            if (IsInRange(CurrentPosition) == false)
            {
                return ReadOnlySpan<char>.Empty;
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

            return _query.Slice(startIndex, length);
        }
        
        /// <summary>
        /// Reads the query while predicate returns true.
        /// </summary>
        public ReadOnlySpan<char> ReadWhile<T>(T state, Func<T, char, bool> predicate)
        {
            if (IsInRange(CurrentPosition) == false)
            {
                return ReadOnlySpan<char>.Empty;
            }

            var length = 0;
            var startIndex = CurrentPosition;

            while (predicate(state, CurrentChar))
            {
                length++;

                if (MoveNext() == false)
                {
                    break;
                }
            }

            return _query.Slice(startIndex, length);
        }
    }
}