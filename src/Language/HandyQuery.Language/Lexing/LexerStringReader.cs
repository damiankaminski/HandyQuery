﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HandyQuery.Language.Lexing
{
    /// <summary>
    /// Provides methods to read a string using different scenarios, e.g. till the end of
    /// whitespaces, till the first whitespace.
    /// </summary>
    internal ref struct LexerStringReader
    {
        public LexerStringReader(string query, int position)
        {
            if (position >= query.Length) throw new IndexOutOfRangeException();

            CurrentPosition = position;
            Query = query.AsReadOnlySpan();
            _queryLength = query.Length;

            // TODO: _linesToPositionMap lazy init? there is no need to initialize it if it's gonna be not needed
            CurrentPosition = 0;
            // TODO: Marshal.AllocHGlobal? or maybe create an allocator that would preallocate some buffer
            // on the beggining of each query handling and if went out of space then would allocate on the heap
            // (but only then! having proper size set it should happen very rarely)
            var linesToPositionMap = new int[CountLines()];
            linesToPositionMap[0] = 0;
            for (var lineIndex = 1; MoveToNextLine(); lineIndex++)
                linesToPositionMap[lineIndex] = CurrentPosition;
            CurrentPosition = position;
            _linesToPositionMap = linesToPositionMap;
        }

        public int CurrentPosition { get; private set; }

        public char CurrentChar => Query[CurrentPosition];

        public RelativePositionInfo CurrentRelativePositionInfo => GetRelativePositionInfo(CurrentPosition);

        /// <summary>
        /// Map of lines to position.
        /// Span index is a line number (counted from 0), value is a start position. 
        /// </summary>
        private ReadOnlySpan<int> _linesToPositionMap;

        public readonly ReadOnlySpan<char> Query;
        private readonly int _queryLength;

        /// <summary>
        /// Reads the query till first non whitespace occurrence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillEndOfWhitespace()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            // method group would allocate on heap
            return ReadWhile(c => char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Reads the query till first whitespace occurence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillEndOfWord()
        {
            return ReadWhile(x => char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads the query till finds <see cref="x"/> words separated by whitespaces.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillEndOfNumber(char seperator)
        {
            return ReadWhile(seperator, (sep, x) => char.IsDigit(x) || x == sep);
        }

        /// <summary>
        /// Reads the query till the first occurrance of invalid char.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillIvalidChar(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            return ReadWhile(invalidChars, (chars, x) => chars.Contains(x) == false);
        }

        /// <summary>
        /// Reads the query till the first occurrance of invalid char.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillIvalidCharOrWhitespace(IEnumerable<char> invalidChars)
        {
            // TODO: use Span instead of IEnumerable?
            return ReadWhile(invalidChars, (chars, x) => chars.Contains(x) == false && char.IsWhiteSpace(x) == false);
        }

        /// <summary>
        /// Reads until new line character is found.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadTillNewLine()
        {
            return ReadWhile(x => x != '\n' && x != '\r');
        }

        /// <summary>
        /// Determines whether query in <see cref="CurrentPosition"/> starts with given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

                if (Query[pos] != value[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks wheter <see cref="CurrentPosition"/> is in range of query.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInRange() => IsInRange(CurrentPosition);

        /// <summary>
        /// Checks wheter given <see cref="position"/> is in range of query.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInRange(int position) => _queryLength > position && position >= 0;

        /// <summary>
        /// Checks wheter everything has been already read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEndOfQuery() => IsInRange(CurrentPosition + 1) == false;

        /// <summary>
        /// Moves to next char.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => MoveBy(1);

        /// <summary>
        /// Moves <see cref="CurrentPosition"/> by <see cref="x"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Moves <see cref="CurrentPosition"/> until passes new line.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEndOfLine()
        {
            return CurrentChar == '\r' || CurrentChar == '\n';
        }

        /// <summary>
        /// Reads the query while predicate returns true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            return Query.Slice(startIndex, length);
        }

        /// <summary>
        /// Reads the query while predicate returns true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            return Query.Slice(startIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Position CaptureCurrentPosition()
        {
            return new Position(CurrentPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveTo(Position position)
        {
            CurrentPosition = position.Value;
        }

        private int CountLines()
        {
            var position = CurrentPosition;

            CurrentPosition = 0;

            var lines = 1;
            while (MoveToNextLine()) lines++;

            CurrentPosition = position;

            return lines;
        }

        public RelativePositionInfo GetRelativePositionInfo(int position)
        {
            // TODO: use different strategy for larger queries (binary search?)

            if (_linesToPositionMap.Length == 1)
            {
                return new RelativePositionInfo(1, position + 1);
            }

            var nextLinePosition = 0;
            var line = 0;
            do
            {
                nextLinePosition = _linesToPositionMap[line + 1];
                line++;
            } while (nextLinePosition <= position && _linesToPositionMap.Length > line + 1);

            if (nextLinePosition <= position)
            {
                // line not found yet then it is last line
                line++;
            }

            var linePosition = _linesToPositionMap[line - 1];

            return new RelativePositionInfo(line, position - linePosition + 1);
        }

        internal readonly ref struct RelativePositionInfo
        {
            public readonly int Line;
            public readonly int Column;

            public RelativePositionInfo(int line, int column)
            {
                Line = line;
                Column = column;
            }
        }

        internal readonly ref struct Position
        {
            public readonly int Value;

            public Position(int value)
            {
                Value = value;
            }
        }

        internal ref struct Restorable
        {
            private int _capturedPosition;

            /// <summary>
            /// Saves current value so that it could be restored later on with <see cref="RestorePosition"/> method.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CaptureCurrentPosition(ref LexerStringReader reader)
            {
                _capturedPosition = reader.CurrentPosition;
            }

            /// <summary>
            /// Restores value to latest captured one. If value was not ever captured then restores to 0.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RestorePosition(ref LexerStringReader reader)
            {
                reader.CurrentPosition = _capturedPosition;
            }
        }
    }
}