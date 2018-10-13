using System;
using System.Collections.Generic;

namespace HandyQuery.Language.Lexing
{
    internal readonly struct LexerStringPositionProvider
    {
        /// <summary>
        /// Map of lines to position.
        /// Index is a line number (counted from 0), value is a start position. 
        /// </summary>
        private readonly List<int> _linesToPositionMap;

        private LexerStringPositionProvider(List<int> linesToPositionMap)
        {
            _linesToPositionMap = linesToPositionMap;
        }
        
        public static LexerStringPositionProvider Create(LexerStringReader reader)
        {
            reader.MoveTo(new LexerStringReader.Position(0));

            // TODO: heap alloc :(
            var linesToPositionMap = new List<int>(10) {0};

            while(reader.MoveToNextLine())
                linesToPositionMap.Add(reader.CurrentPosition);

            return new LexerStringPositionProvider(linesToPositionMap);
        }

        public RelativePositionInfo GetRelativePositionInfo(int position)
        {
            // TODO: use different strategy for larger queries (binary search?)

            if (_linesToPositionMap.Count == 1)
            {
                return new RelativePositionInfo(1, position + 1);
            }

            var nextLinePosition = 0;
            var line = 0;
            do
            {
                nextLinePosition = _linesToPositionMap[line + 1];
                line++;
            } while (nextLinePosition <= position && _linesToPositionMap.Count > line + 1);

            if (nextLinePosition <= position)
            {
                // line not found yet then it is last line
                line++;
            }

            var linePosition = _linesToPositionMap[line - 1];

            return new RelativePositionInfo(line, position - linePosition + 1);
        }
    }
}