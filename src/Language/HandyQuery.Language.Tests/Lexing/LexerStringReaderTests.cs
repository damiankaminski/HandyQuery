using System;
using System.Linq;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing
{
    public class LexerStringReaderTests
    {
        [Test]
        public void Should_allow_to_move_through_query()
        {
            var reader = CreateReader("|Test");

            reader.IsInRange(0).Should().Be(true);
            reader.IsInRange(3).Should().Be(true);
            reader.IsInRange(4).Should().Be(false);
            
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
            
            reader.MoveNext().Should().Be(true);
            reader.CurrentPosition.Should().Be(1);
            reader.CurrentChar.Should().Be('e');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
            
            reader.MoveBy(2).Should().Be(true);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(true);

            reader.MoveNext().Should().Be(false);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(true);
            
            reader.MoveBy(2).Should().Be(false);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(true);
            
            reader.MoveBy(-1).Should().Be(true);
            reader.CurrentPosition.Should().Be(2);
            reader.CurrentChar.Should().Be('s');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
            
            reader.MoveBy(-2).Should().Be(true);
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
            
            reader.MoveBy(-1).Should().Be(false);
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
            
            reader.MoveBy(-2).Should().Be(false);
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');
            reader.IsInRange().Should().Be(true);
            reader.IsEndOfQuery().Should().Be(false);
        }
        
        [Test]
        public void Should_allow_to_capture_and_restore_positions()
        {
            var restorableReader = new LexerStringReader.Restorable();
            var reader = CreateReader("Te|st");
            reader.CurrentPosition.Should().Be(2);
            reader.CurrentChar.Should().Be('s');
            
            restorableReader.RestorePosition(ref reader);
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');

            reader.MoveBy(3);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            
            restorableReader.CaptureCurrentPosition(ref reader);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            
            reader.MoveBy(-1);
            reader.CurrentPosition.Should().Be(2);
            reader.CurrentChar.Should().Be('s');
            
            restorableReader.RestorePosition(ref reader);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
        }
        
        [Test]
        public void Should_allow_to_capture_and_move_to()
        {
            var reader = CreateReader("Te|st");
            reader.CurrentPosition.Should().Be(2);
            reader.CurrentChar.Should().Be('s');
            
            reader.MoveTo(new LexerStringReader.Position(0));
            reader.CurrentPosition.Should().Be(0);
            reader.CurrentChar.Should().Be('T');

            reader.MoveBy(3);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            
            var position = reader.CaptureCurrentPosition();
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
            
            reader.MoveBy(-1);
            reader.CurrentPosition.Should().Be(2);
            reader.CurrentChar.Should().Be('s');
            
            reader.MoveTo(position);
            reader.CurrentPosition.Should().Be(3);
            reader.CurrentChar.Should().Be('t');
        }

        [Test]
        public void Should_expose_current_line_and_column()
        {
            var reader = CreateReader("|Test\nTest2\nTest3");

            reader.CurrentPosition.Should().Be(0);
            reader.CurrentRelativePositionInfo.Line.Should().Be(1);
            reader.CurrentRelativePositionInfo.Column.Should().Be(1);

            reader.MoveToNextLine().Should().Be(true);
            reader.CurrentPosition.Should().Be(5);
            reader.CurrentRelativePositionInfo.Line.Should().Be(2);
            reader.CurrentRelativePositionInfo.Column.Should().Be(1);
            
            reader.MoveBy(2).Should().Be(true);
            reader.CurrentPosition.Should().Be(7);
            reader.CurrentRelativePositionInfo.Line.Should().Be(2);
            reader.CurrentRelativePositionInfo.Column.Should().Be(3);
            
            reader.MoveBy(3).Should().Be(true);
            reader.CurrentPosition.Should().Be(10);
            reader.CurrentRelativePositionInfo.Line.Should().Be(2);
            reader.CurrentRelativePositionInfo.Column.Should().Be(6);
            
            reader.MoveToNextLine().Should().Be(true);
            reader.CurrentPosition.Should().Be(11);
            reader.CurrentRelativePositionInfo.Line.Should().Be(3);
            reader.CurrentRelativePositionInfo.Column.Should().Be(1);
            
            reader.MoveBy(4).Should().Be(true);
            reader.CurrentPosition.Should().Be(15);
            reader.CurrentRelativePositionInfo.Line.Should().Be(3);
            reader.CurrentRelativePositionInfo.Column.Should().Be(5);
        }
        
        [TestCase(1, 0, 1)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 0, 1)]
        [TestCase(2, 1, 2)]
        [TestCase(10, 1, 2)]
        [TestCase(10, 2, 3)]
        [TestCase(10, 5, 6)]
        [TestCase(10, 9, 10)]
        [TestCase(10, 10, 10)]
        [TestCase(10, 11, 10)]
        [TestCase(100, 50, 51)]
        [TestCase(100, 100, 100)]
        [TestCase(100, 101, 100)]
        [TestCase(107, 105, 106)]
        [TestCase(107, 106, 107)]
        [TestCase(107, 107, 107)]
        public void Should_calculate_current_line_correctly(int lines, int moveToNextLineTimes, int expectedLine)
        {
            var query = "|" + string.Join('\n', Enumerable.Range(0, lines).Select(x => $"Test{x}"));
            var reader = CreateReader(query);

            for (int i = 0; i < moveToNextLineTimes; i++)
            {
                reader.MoveToNextLine();
            }

            reader.CurrentRelativePositionInfo.Line.Should().Be(expectedLine);
            reader.CurrentRelativePositionInfo.Column.Should().Be(1);
        }
        
        [TestCase("Some kind of 'query|'", false, null)]
        [TestCase("|Some kind of 'query'", false, null)]
        [TestCase("|Some kind of 'query'\n", false, null)]
        [TestCase("|Some kind of 'query'\r\n", false, null)]
        [TestCase("|Some \rkind of 'query'", true, 'k')]
        [TestCase("|Some \r\nkind of 'query'", true, 'k')]
        [TestCase("|Some \nkind of 'query'", true, 'k')]
        [TestCase("|Some \r kind of 'query'", true, ' ')]
        [TestCase("|Some \r\n kind of 'query'", true, ' ')]
        [TestCase("|Some \r \nkind of 'query'", true, ' ')]
        [TestCase("|Some \n kind of 'query'", true, ' ')]
        [TestCase("|Some \nt\r\n kind of 'query'", true, 't')]
        [TestCase("S|ome \rkind of 'query'", true, 'k')]
        [TestCase("So|me \r\nkind of 'query'", true, 'k')]
        [TestCase("Some| \nkind of 'query'", true, 'k')]
        [TestCase("Some |\nkind of 'query'", true, 'k')]
        public void Method_MoveToNextLine_should_work_properly(string query, bool expectedResult, char expectedChar)
        {
            var reader = CreateReader(query);
            
            var result = reader.MoveToNextLine();

            result.Should().Be(expectedResult);
            if (expectedResult) reader.CurrentChar.Should().Be(expectedChar);
        }
        
        [TestCase("Some kind of 'query|'", '\'', "")]
        [TestCase("|Some kind of 'query'", '\'', "Some kind of ")]
        [TestCase("Some |kind of 'query'", '\'', "kind of ")]
        [TestCase("Some| kind of 'query'", '\'', " kind of ")]
        [TestCase("Some kind of |'query'", '"', "'query'")]
        public void Method_ReadWhile_should_work_properly(string query, char invalidChar, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadWhile(x => x != invalidChar);

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("Some    kind of 'query|'", "")]
        [TestCase("Some|    kind of 'query'", "    ")]
        [TestCase("S|ome    kind of 'query'", "")]
        [TestCase("Some |   kind of 'query'", "   ")]
        [TestCase("Some| \n\r \t  kind of 'query'", " \n\r \t  ")]
        public void Method_ReadTillEndOfWhitespace_should_work_properly(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfWhitespace();

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("Some kind\nof 'query|'", "'")]
        [TestCase("Some|    kind of 'query'", "")]
        [TestCase("S|ome    kind of 'query'", "ome")]
        [TestCase("Some    |kind of 'query'", "kind")]
        [TestCase("Some |kind\nof 'query'", "kind")]
        [TestCase("Some kind\nof |'query'", "'query'")]
        public void Method_ReadTillEndOfWord_should_work_properly(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfWord();

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("10/12/2017 08:30:2|1", 2, "1")]
        [TestCase("Some|    kind of 'query'", 2, "    kind")]
        [TestCase("S|ome    kind of 'query'", 0, "")]
        [TestCase("S|ome    kind of 'query'", 1, "ome")]
        [TestCase("S|ome    kind of 'query'", 2, "ome    kind")]
        [TestCase("S|ome    kind of 'query'", 3, "ome    kind of")]
        [TestCase("Some    |kind of 'query'", 2, "kind of")]
        [TestCase("Some |kind\nof 'query'", 2, "kind\nof")]
        [TestCase("Some kind\nof |'query'", 2, "'query'")]
        [TestCase("|10/12/2017 08:30:21 test", 2, "10/12/2017 08:30:21")]
        [TestCase("|10/12/2017 08:30:21", 2, "10/12/2017 08:30:21")]
        public void Method_ReadTillEndOfXWords_should_work_properly(string query, int x, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfXWords(x);

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("some 123,45|6", '.', "6")]
        [TestCase("|123 kind", '.', "123")]
        [TestCase("|123.456 kind", '.', "123.456")]
        [TestCase("|123,456 kind", ',', "123,456")]
        [TestCase("some |123 kind", '.', "123")]
        [TestCase("some |123.456 kind", '.', "123.456")]
        [TestCase("some |123,456 kind", ',', "123,456")]
        [TestCase("some |123", '.', "123")]
        [TestCase("some |123.456", '.', "123.456")]
        [TestCase("some |123,456", ',', "123,456")]
        public void Method_ReadTillEndOfNumber_should_work_properly(string query, char separator, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfNumber(separator);

            new string(result).Should().Be(expectedResult);
        }

        [TestCase("Some kind of 'query|'", "")]
        [TestCase("|Some kind of 'query'", "Some kind of ")]
        [TestCase("Some |kind of 'query'", "kind of ")]
        [TestCase("Some |kind of \"query\"", "kind of ")]
        [TestCase("Some| kind of 'query'", " kind of ")]
        [TestCase("Some kind of |'query'", "")]
        public void Method_ReadTillIvalidChar_should_work_properly(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            var invalidChars = new []{'\'', '"'};
            
            var result = reader.ReadTillIvalidChar(invalidChars);

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("Some kind of 'query|'", "")]
        [TestCase("|Some kind of 'query'", "Some")]
        [TestCase("Some |kind of 'query'", "kind")]
        [TestCase("Some |kindof'query'", "kindof")]
        [TestCase("Some |kind of \"query\"", "kind")]
        [TestCase("Some |kindof\"query\"", "kindof")]
        [TestCase("Some| kind of 'query'", "")]
        [TestCase("Some kind of |'query'", "")]
        public void Method_ReadTillIvalidCharOrWhitespace_should_work_properly(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            var invalidChars = new []{'\'', '"'};
            
            var result = reader.ReadTillIvalidCharOrWhitespace(invalidChars);

            new string(result).Should().Be(expectedResult);
        }
        
        [TestCase("Some kind of 'query|'", "'", '\'')]
        [TestCase("|Some kind of 'query'", "Some kind of 'query'", '\'')]
        [TestCase("Some |kind of 'query'", "kind of 'query'", '\'')]
        [TestCase("Some |kind \nof 'query'", "kind ", '\n')]
        [TestCase("|Some kind \r\nof 'query'", "Some kind ", '\r')]
        public void Method_ReadTillNewLine_should_work_properly(string query, string expectedResult, char stoppedAt)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillNewLine();

            new string(result).Should().Be(expectedResult);
            reader.CurrentChar.Should().Be(stoppedAt);
        }
        
        [TestCase("Some kind of 'query|'", "", true)]
        [TestCase("Some kind of 'query|'", "Some", false)]
        [TestCase("Some kind of 'query'|tes", "test", false)]
        [TestCase("Some kind of 'query'|test", "test", true)]
        [TestCase("Some kind of 'query'|testf", "test", true)]
        [TestCase("|Some kind of 'query'", "Some", true)]
        [TestCase("Some |kind of 'query'", "kind", true)]
        [TestCase("Some |kind \nof 'query'", "kind ", true)]
        [TestCase("|Some kind \r\nof 'query'", "Some kind \r\n", true)]
        [TestCase("|Some kind \r\nof 'query'", "kind", false)]
        public void Method_StartsWith_should_work_properly(string query, string value, bool expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.StartsWith(value.AsReadOnlySpan());

            result.Should().Be(expectedResult);
        }
        
        private static LexerStringReader CreateReader(string query)
        {
            var caretIndex = query.IndexOf("|", StringComparison.Ordinal);
            var withoutCaret = $"{query.Substring(0, caretIndex)}{query.Substring(caretIndex + 1)}";
            
            return new LexerStringReader(withoutCaret, caretIndex);
        }
    }
}