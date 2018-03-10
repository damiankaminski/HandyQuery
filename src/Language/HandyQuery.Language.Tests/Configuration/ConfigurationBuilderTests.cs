using System;
using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Configuration
{
    // TODO: test all supported types
    
    public class ConfigurationBuilderTests
    {
        [Test]
        public void Should_be_able_to_build_basic_configuration()
        {
            var syntax = HandyQueryLanguage.BuildSyntax()
                .WithCaseSensitiveKeywords()
                .WithCaseSensitiveColumnNames();

            var configurationBuilder = HandyQueryLanguage.Configure<Person>(syntax)
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName)
                .AddColumn("MiddleName", x => x.LastName);

            var configuration = configurationBuilder.Build();

            configuration.Should().BeEquivalentTo(new LanguageConfig(typeof(Person), new List<ColumnInfo>()
            {
                new ColumnInfo("FirstName", "FirstName", typeof(string)),
                new ColumnInfo("LastName", "LastName", typeof(string)),
                new ColumnInfo("MiddleName", "LastName", typeof(string))
            }, new SyntaxConfig(true, true)));
        }
        
        [Test]
        public void Should_allow_to_build_multiple_times()
        {
            var configurationBuilder = HandyQueryLanguage.Configure<Person>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName);

            configurationBuilder.Build();
            configurationBuilder.Build();
            configurationBuilder.Build();
            var configuration = configurationBuilder.Build();

            configuration.Should().BeEquivalentTo(new LanguageConfig(typeof(Person), new List<ColumnInfo>()
            {
                new ColumnInfo("FirstName", "FirstName", typeof(string)),
                new ColumnInfo("LastName", "LastName", typeof(string))
            }, new SyntaxConfig(false, false)));
        }
        
        [Test]
        public void Should_support_fields_as_columns()
        {
            var configurationBuilder = HandyQueryLanguage.Configure<TestModel>()
                .AddColumn(x => x.Salary);

            var configuration = configurationBuilder.Build();

            configuration.Should().BeEquivalentTo(new LanguageConfig(typeof(TestModel), new List<ColumnInfo>()
            {
                new ColumnInfo("Salary", "Salary", typeof(int)),
            }, new SyntaxConfig(false, false)));
        }
        
        [Test]
        public void Should_support_nested_members_as_columns()
        {
            var configurationBuilder = HandyQueryLanguage.Configure<TestModel>()
                .AddColumn(x => x.Salary)
                .AddColumn(x => x.Person.FirstName)
                .AddColumn(x => x.Person.LastName)
                .AddColumn("MiddleName", x => x.Person.LastName);

            var configuration = configurationBuilder.Build();

            configuration.Should().BeEquivalentTo(new LanguageConfig(typeof(TestModel), new List<ColumnInfo>()
            {
                new ColumnInfo("Salary", "Salary", typeof(int)),
                new ColumnInfo("FirstName", "Person.FirstName", typeof(string)),
                new ColumnInfo("LastName", "Person.LastName", typeof(string)),
                new ColumnInfo("MiddleName", "Person.LastName", typeof(string))
            }, new SyntaxConfig(false, false)));
        }
        
        [TestCase(">MiddleName", ">")]
        [TestCase("Middle>Name", ">")]
        [TestCase("Middl>eName", ">")]
        [TestCase("MiddleName>", ">")]
        [TestCase("=MiddleName", "=")]
        [TestCase("Middle=Name", "=")]
        [TestCase("Middl=eName", "=")]
        [TestCase("MiddleName=", "=")]
        public void Should_throw_if_column_name_is_not_valid(string columnName, string invalidChar)
        {
            var configurationBuilder = HandyQueryLanguage.Configure<Person>()
                .AddColumn(columnName, x => x.LastName);

            Action action = () => configurationBuilder.Build();

            action.Should()
                .ThrowExactly<ConfigurationException>()
                .WithMessage($"Column name ('{columnName}') contains invalid character: {invalidChar}.")
                .And.ExceptionType.Should().Be(ConfigurationExceptionType.InvalidColumnName);
        }

        [Test]
        public void Should_throw_if_column_name_is_duplicated()
        {
            var configurationBuilder = HandyQueryLanguage.Configure<Person>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName);

            Action action = () => configurationBuilder.AddColumn(x => x.FirstName);

            action.Should()
                .ThrowExactly<ConfigurationException>()
                .WithMessage("Column named as 'FirstName' is defined twice.")
                .And.ExceptionType.Should().Be(ConfigurationExceptionType.DuplicatedColumnName);
        }

        [Test]
        public void Should_throw_if_column_name_accessor_is_invalid()
        {
            var configurationBuilder = HandyQueryLanguage.Configure<Person>();

            Action action = () => configurationBuilder.AddColumn(x => x.GetHashCode());
            Action action2 = () => configurationBuilder.AddColumn("Test", x => x.GetHashCode());

            var expected = "Invalid column name definition. 'propertyOrField' argument needs to return a property " +
                           "or field and nothing else. Currently defined as: x => Convert(x.GetHashCode(), Object)";
            action.Should().ThrowExactly<ConfigurationException>().WithMessage(expected)
                .And.ExceptionType.Should().Be(ConfigurationExceptionType.InvalidColumnNameMemberDefinition);
            action2.Should().ThrowExactly<ConfigurationException>().WithMessage(expected)
                .And.ExceptionType.Should().Be(ConfigurationExceptionType.InvalidColumnNameMemberDefinition);
        }

        private class TestModel
        {
            public readonly int Salary;
            public PersonInfo Person { get; }
            
            public TestModel(PersonInfo person, int salary)
            {
                Person = person;
                Salary = salary;
            }
            
            public class PersonInfo
            {
                public readonly string FirstName;
                public string LastName { get; }
                
                public PersonInfo(string lastName, string firstName)
                {
                    LastName = lastName;
                    FirstName = firstName;
                }
            }
        }
    }
}