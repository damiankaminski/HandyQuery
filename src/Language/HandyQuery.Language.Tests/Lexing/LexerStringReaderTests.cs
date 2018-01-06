using System;
using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing
{
    public class LexerStringReaderTests
    {
        [TestCase("Some kind of 'query'|", '\'', "")]
        [TestCase("|Some kind of 'query'", '\'', "Some kind of ")]
        [TestCase("Some |kind of 'query'", '\'', "kind of ")]
        [TestCase("Some| kind of 'query'", '\'', " kind of ")]
        [TestCase("Some kind of |'query'", '"', "'query'")]
        public void ReadWhile(string query, char invalidChar, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadWhile(x => x != invalidChar);

            result.Should().Be(expectedResult);
        }
        
        [TestCase("Some    kind of 'query'|", "")]
        [TestCase("Some|    kind of 'query'", "    ")]
        [TestCase("S|ome    kind of 'query'", "")]
        [TestCase("Some |   kind of 'query'", "   ")]
        [TestCase("Some| \n\r \t  kind of 'query'", " \n\r \t  ")]
        public void ReadTillEndOfWhitespace(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillEndOfWhitespace();

            result.Should().Be(expectedResult);
        }
        
        [TestCase("Some kind\nof 'query'|", "")]
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
        }
        
        [TestCase("10/12/2017 08:30:21|", 2, "")]
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
        }
        
        [TestCase("some 123,456|", '.', "")]
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
        }

        [TestCase("Some kind of 'query'|", "")]
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
        }
        
        [TestCase("Some kind of 'query'|", "")]
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
        }
        
        [TestCase("Some kind of 'query'|", "")]
        [TestCase("|Some kind of 'query'", "Some kind of 'query'")]
        [TestCase("Some |kind of 'query'", "kind of 'query'")]
        [TestCase("Some |kind \nof 'query'", "kind ")]
        [TestCase("|Some kind \r\nof 'query'", "Some kind ")]
        public void ReadTillNewLine(string query, string expectedResult)
        {
            var reader = CreateReader(query);
            
            var result = reader.ReadTillNewLine();

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