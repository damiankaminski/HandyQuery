using System.Collections.Generic;
using FluentAssertions;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Tests.Model;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Configuration
{
    public class ConfigurationBuilderTests
    {
        // TODO: moar tests
        
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
            
            configuration.ShouldBeEquivalentTo(new LanguageConfig(typeof(Person), new List<ColumnInfo>()
            {
                new ColumnInfo("FirstName", "FirstName", typeof(string)),
                new ColumnInfo("LastName", "LastName", typeof(string)),
                new ColumnInfo("MiddleName", "LastName", typeof(string))
            }, new SyntaxConfig(true, true)));
        }
    }
}