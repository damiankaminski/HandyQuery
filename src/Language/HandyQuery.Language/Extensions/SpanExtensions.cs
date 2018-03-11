using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HandyQuery.Language.Extensions
{
    // TODO: maybe some of it should be included in the corefx?
    
    internal static class SpanExtensions
    {
        // TODO: tests
        public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
            {
                return true;
            }

            if (span.Length == 0)
            {
                return false;
            }
            
            if (value.Length > span.Length)
            {
                return false;
            }

            var slice = span.Slice(span.Length - value.Length, value.Length);
            if (slice.Length < span.Length)
            {
                return false;
            }
            
            for (var i = 0; i < slice.Length; i++)
            {
                if (slice[i] != value[i]) return false;
            }

            return true;
        }
        
        // TODO: tests
        public static bool EndsWith(this ReadOnlySpan<char> span, char value)
        {
            if (span.Length == 0)
            {
                return false;
            }

            return span[span.Length - 1] == value;
        }

        internal class SplitItem
        {
            private readonly int _startIndex;
            private readonly int _length;

            public SplitItem(int startIndex, int length)
            {
                _startIndex = startIndex;
                _length = length;
            }

            // TODO: could also store a pointer to stack memory and then "span" param would be not needed
            // TODO: new SplitItem(startIndex, length, &span.DangerousGetPinnableReference(), span.length);
            // TODO: rename then to DangerousSlice?
            public ReadOnlySpan<char> SliceFrom(ref ReadOnlySpan<char> span)
            {
                return span.Slice(_startIndex, _length);
            }
        }
        
        // TODO: tests
        [HeapAlloc]
        public static IEnumerable<SplitItem> Split(this ReadOnlySpan<char> span, char value, 
            StringSplitOptions options)
        {
            if (span.Length == 0)
            {
                return Enumerable.Empty<SplitItem>();
            }
            
            var items = new List<SplitItem>(8);
            var startIndex = 0;

            if (options == StringSplitOptions.None)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] != value) continue;
                    
                    var length = i - startIndex;
                    items.Add(new SplitItem(startIndex, length));
                    startIndex = i + 1;
                }
            }
            else
            {
                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] != value) continue;
                    if (i == startIndex) continue;
                    
                    var length = i - startIndex;
                    items.Add(new SplitItem(startIndex, length));
                    startIndex = i + 1;
                }
            }

            if (startIndex != span.Length - 1)
            {
                items.Add(new SplitItem(startIndex, span.Length - startIndex));   
            }
            
            return items.ToArray();
        }
    }
}