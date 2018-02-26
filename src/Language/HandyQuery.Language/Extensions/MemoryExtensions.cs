// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace System
{
    /// <summary>
    /// Extension methods for Span{T}, Memory{T}, and friends.
    /// </summary>
    internal static partial class MemoryExtensions
    {
        /// <summary>
        /// Removes all leading and trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The span</param>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span)
        {
            return span.TrimStart().TrimEnd();
        }

        /// <summary>
        /// Removes all leading white-space characters from the span.
        /// </summary>
        /// <param name="span">The span</param>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (!char.IsWhiteSpace(span[start]))
                    break;
            }
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing white-space characters from the span.
        /// </summary>
        /// <param name="span">The span</param>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                    break;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, char trimChar)
        {
            return span.TrimStart(trimChar).TrimEnd(trimChar);
        }

        /// <summary>
        /// Removes all leading occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, char trimChar)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                if (span[start] != trimChar)
                    break;
            }
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a specified character.
        /// </summary>
        /// <param name="span">The source span from which the character is removed.</param>
        /// <param name="trimChar">The specified character to look for and remove.</param>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, char trimChar)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (span[end] != trimChar)
                    break;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters specified 
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            return span.TrimStart(trimChars).TrimEnd(trimChars);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of characters specified 
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            int start = 0;
            for (; start < span.Length; start++)
            {
                for (int i = 0; i < trimChars.Length; i++)
                {
                    if (span[start] == trimChars[i])
                        goto Next;
                }
                break;
            Next: ;
            }
            return span.Slice(start);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of characters specified 
        /// in a readonly span from the span.
        /// </summary>
        /// <param name="span">The source span from which the characters are removed.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                for (int i = 0; i < trimChars.Length; i++)
                {
                    if (span[end] == trimChars[i])
                        goto Next;
                }
                break;
            Next: ;
            }
            return span.Slice(0, end + 1);
        }

        /// <summary>
        /// Indicates whether the specified span contains only white-space characters.
        /// </summary>
        public static bool IsWhiteSpace(this ReadOnlySpan<char> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (!char.IsWhiteSpace(span[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a new  span over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this T[] array)
        {
            return new Span<T>(array);
        }

        /// <summary>
        /// Creates a new  span over the portion of the target array segment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> arraySegment)
        {
            return new Span<T>(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
        }

        /// <summary>
        /// Creates a new readonly span over the entire target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array)
        {
            return new ReadOnlySpan<T>(array);
        }

        /// <summary>
        /// Creates a new readonly span over the entire target span.
        /// </summary>
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span) => span;

        /// <summary>
        /// Creates a new readonly span over the target array segment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ArraySegment<T> arraySegment)
        {
            return new ReadOnlySpan<T>(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
        }

        /// <summary>
        /// Copies the contents of the array into the span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// 
        ///<param name="array">The array to copy items from.</param>
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination Span is shorter than the source array.
        /// </exception>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, Span<T> destination)
        {
            new ReadOnlySpan<T>(array).CopyTo(destination);
        }
    }
}
