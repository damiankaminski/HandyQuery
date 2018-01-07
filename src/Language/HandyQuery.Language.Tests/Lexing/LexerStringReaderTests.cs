using System;
using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing
{
    public class LexerStringReaderTests
    {
        [Test]
        public void ShouldAllowToMoveThroughQuery()
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
        public void MoveToNextLine(string query, bool expectedResult, char expectedChar)
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
        public void ReadWhile(string query, char invalidChar, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadWhile(x => x != invalidChar);

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
        }
        
        [TestCase("Some    kind of 'query|'", "")]
        [TestCase("Some|    kind of 'query'", "    ")]
        [TestCase("S|ome    kind of 'query'", "")]
        [TestCase("Some |   kind of 'query'", "   ")]
        [TestCase("Some| \n\r \t  kind of 'query'", " \n\r \t  ")]
        public void ReadTillEndOfWhitespace(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfWhitespace();

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
        }
        
        [TestCase("Some kind\nof 'query|'", "'")]
        [TestCase("Some|    kind of 'query'", "")]
        [TestCase("S|ome    kind of 'query'", "ome")]
        [TestCase("Some    |kind of 'query'", "kind")]
        [TestCase("Some |kind\nof 'query'", "kind")]
        [TestCase("Some kind\nof |'query'", "'query'")]
        public void ReadTillEndOfWord(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfWord();

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
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
        public void ReadTillEndOfXWords(string query, int x, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfXWords(x);

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
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
        public void ReadTillEndOfNumber(string query, char separator, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfNumber(separator);

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
        }

        [TestCase("Some kind of 'query|'", "")]
        [TestCase("|Some kind of 'query'", "Some kind of ")]
        [TestCase("Some |kind of 'query'", "kind of ")]
        [TestCase("Some |kind of \"query\"", "kind of ")]
        [TestCase("Some| kind of 'query'", " kind of ")]
        [TestCase("Some kind of |'query'", "")]
        public void ReadTillIvalidChar(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            var invalidChars = new []{'\'', '"'};
            
            var result = reader.ReadTillIvalidChar(invalidChars);

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
        }
        
        [TestCase("Some kind of 'query|'", "")]
        [TestCase("|Some kind of 'query'", "Some")]
        [TestCase("Some |kind of 'query'", "kind")]
        [TestCase("Some |kindof'query'", "kindof")]
        [TestCase("Some |kind of \"query\"", "kind")]
        [TestCase("Some |kindof\"query\"", "kindof")]
        [TestCase("Some| kind of 'query'", "")]
        [TestCase("Some kind of |'query'", "")]
        public void ReadTillIvalidCharOrWhitespace(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            var invalidChars = new []{'\'', '"'};
            
            var result = reader.ReadTillIvalidCharOrWhitespace(invalidChars);

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
        }
        
        [TestCase("Some kind of 'query|'", "'")]
        [TestCase("|Some kind of 'query'", "Some kind of 'query'")]
        [TestCase("Some |kind of 'query'", "kind of 'query'")]
        [TestCase("Some |kind \nof 'query'", "kind ")]
        [TestCase("|Some kind \r\nof 'query'", "Some kind ")]
        public void ReadTillNewLine(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillNewLine();

            result.Should().Be(expectedResult);
            reader.ReadLength.Should().Be(expectedResult.Length);
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
        public void StartsWith(string query, string value, bool expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.StartsWith(value);

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