using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokenizers;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing.Tokenizers
{
    public class ColumnNameTokenizerTests
    {
        private static readonly LanguageConfig DefaultConfig = HandyQueryLanguage.Configure<Person>()
            .AddColumn("Name", x => x.FirstName)
            .AddColumn("LastName", x => x.LastName)
            .Build();

        private static ColumnNameTokenizer Tokenizer => new ColumnNameTokenizer();

        [Test]
        public void Should_tokenize_simplest_query()
        {
            var query = "Name";
            var columnName = query;
            
            var (result, token) = Tokenize(query);

            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(0);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be("Name");
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }

        [Test]
        public void Should_tokenize_query_with_multiple_words()
        {
            var query = "Name some kind of test";
            var columnName = "Name";
            
            var (result, token) = Tokenize(query);

            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(0);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be(columnName);
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }

        [Test]
        public void Should_tokenize_query_when_in_the_middle()
        {
            var query = "some kind Name of test";
            var columnName = "Name";
            var position = 10;
            
            var (result, token) = Tokenize(query, position);

            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(position);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be(columnName);
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }
        
        [Test]
        public void Should_tokenize_query_when_at_the_end()
        {
            var query = "some kind Name";
            var columnName = "Name";
            var position = 10;
            
            var (result, token) = Tokenize(query, position);

            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(position);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be(columnName);
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }

        [Test]
        public void Should_tokenize_with_case_insensitive_syntax()
        {
            var query = "nAmE some test";
            var columnName = "Name";
            
            var (result, token) = Tokenize(query, 0, DefaultConfig);

            DefaultConfig.Syntax.ColumnNameCaseSensitive.Should().Be(false);
            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(0);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be(columnName);
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }
        
        [Test]
        public void Should_tokenize_till_first_invalid_char()
        {
            var query = "name!kind of test query";
            var columnName = "Name";
            
            var (result, token) = Tokenize(query);

            result.Success.Should().BeTrue();
            result.Token.TokenType.Should().Be(TokenType.Column);
            token.StartPosition.Should().Be(0);
            token.Length.Should().Be(columnName.Length);
            token.ColumnInfo.ColumnName.Should().Be(columnName);
            token.ColumnInfo.MemberName.Should().Be("FirstName");
            token.ColumnInfo.SystemType.Should().Be(typeof(Person).GetProperty("FirstName").PropertyType);
        }
        
        [Test]
        public void Should_fail_if_syntax_is_case_sensitive_and_given_query_is_not()
        {
            var syntax = HandyQueryLanguage.BuildSyntax().WithCaseSensitiveColumnNames();
            var config = HandyQueryLanguage.Configure<Person>(syntax)
                .AddColumn("Name", x => x.FirstName)
                .Build();
            var query = "nAmE some test";
            
            var (result, _) = Tokenize(query, 0, config);

            syntax.Build().ColumnNameCaseSensitive.Should().Be(true);
            result.Success.Should().Be(false);
            result.Token.Should().BeNull();
            result.Error.Id.Should().Be(ErrorId.ColumnNotFound);
            result.Error.Range.Position.Should().Be(0);
            result.Error.Range.Length.Should().Be(4);
        }

        [Test]
        public void Should_fail_if_column_not_found()
        {
            var query = "some kind of test query";
            
            var (result, _) = Tokenize(query);

            result.Success.Should().Be(false);
            result.Token.Should().BeNull();
            result.Error.Id.Should().Be(ErrorId.ColumnNotFound);
            result.Error.Range.Position.Should().Be(0);
            result.Error.Range.Length.Should().Be(4);
        }
        
        [Test]
        public void Should_set_correct_error_position_when_in_middle_of_query()
        {
            var query = "some kinda of test query";
            
            var (result, _) = Tokenize(query, 5);

            result.Success.Should().Be(false);
            result.Token.Should().BeNull();
            result.Error.Id.Should().Be(ErrorId.ColumnNotFound);
            result.Error.Range.Position.Should().Be(5);
            result.Error.Range.Length.Should().Be(5);
        }
        
        [Test]
        public void Should_set_correct_error_position_when_in_second_line()
        {
            var position = 15;
            var query = "some kind\nsome kinda of test query";
            
            var (result, _) = Tokenize(query, position);

            result.Success.Should().Be(false);
            result.Token.Should().BeNull();
            result.Error.Id.Should().Be(ErrorId.ColumnNotFound);
            result.Error.Range.Position.Should().Be(position);
            result.Error.Range.Length.Should().Be(5);
        }
        
        private static (TokenizationResult result, ColumnToken token) Tokenize(string query, int position = 0,
            LanguageConfig config = null)
        {
            config = config ?? DefaultConfig;
            var reader = new LexerStringReader(query, position);
            var runtimeInfo = new LexerRuntimeInfo(reader, config);
            var result = Tokenizer.Tokenize(ref runtimeInfo);
            return (result, result.Token as ColumnToken);
        }
    }
}