using System.Collections.Generic;
using System.Text;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokens;

namespace HandyQuery.Language.Tests.Lexing
{
    internal class TestQueryBuilder
    {
        private readonly List<PartBase> _queryParts = new List<PartBase>();
        
        public ColumnPart Column(string columnName)
        {
            var part = new ColumnPart(columnName, this);
            _queryParts.Add(part);
            return part;
        }

        public IEnumerable<BuildResult> BuildMultipleVariants(LanguageConfig languageConfig)
        {
            // TODO: instead of receving languageConfig build one here with different settings?
            
            // most standard case
            {
                var query = new StringBuilder();
                var tokens = new TokenList();

                var index = 0;
                foreach (var part in _queryParts)
                {
                    switch (part)
                    {
                        case ColumnPart column:
                            query.Append(column.ColumnName);
                            query.Append(" ");
                            tokens.Add(new TextLiteralToken(index, column.ColumnName.Length, column.ColumnName));
                            index += column.ColumnName.Length + 1;
                            break;
                        case StatementPart statement:
                            var keyword = languageConfig.Syntax.KeywordsMap[statement.StatementKeyword];
                            query.Append(keyword);
                            query.Append(" ");
                            tokens.Add(new KeywordToken(index, keyword.Length, statement.StatementKeyword));
                            index += keyword.Length + 1;
                            break;
                    }
                }

                query = query.Remove(query.Length - 1, 1); // removes white space
                
                yield return new BuildResult(query.ToString(), tokens);
            }
            
            // TODO: it should allow to build multiple variants automatically
            // e.g. ["FirstName = 10", "FirstName=10", "FIRSTNAME = 10"]
        }

        public class BuildResult
        {
            public TokenList ExpectedTokens { get; }
            public string Query { get; }
            
            public BuildResult(string query, TokenList expectedTokens)
            {
                Query = query;
                ExpectedTokens = expectedTokens;
            }
        }

        public abstract class PartBase
        {
            public TestQueryBuilder TestQueryBuilder { get; }

            protected PartBase(TestQueryBuilder testQueryBuilder)
            {
                TestQueryBuilder = testQueryBuilder;
            }
        }

        public class ColumnPart : PartBase
        {
            public readonly string ColumnName;
            
            public ColumnPart(string columnName, TestQueryBuilder testQueryBuilder) : base(testQueryBuilder)
            {
                ColumnName = columnName;
            }

            public StatementPart IsEmpty()
            {
                return BuildStatementPart(StatementKeyword.IsEmpty);
            }
            
            public StatementPart IsNotEmpty()
            {
                return BuildStatementPart(StatementKeyword.IsNotEmpty);
            }
            
            public StatementPart IsTrue()
            {
                return BuildStatementPart(StatementKeyword.IsTrue);
            }
            
            public StatementPart IsFalse()
            {
                return BuildStatementPart(StatementKeyword.IsFalse);
            }

            private StatementPart BuildStatementPart(StatementKeyword statementKeyword)
            {
                var part = new StatementPart(statementKeyword, TestQueryBuilder);
                TestQueryBuilder._queryParts.Add(part);
                return part;
            }
        }
        
        public class StatementPart : PartBase
        {
            public readonly StatementKeyword StatementKeyword;

            public StatementPart(StatementKeyword statementKeyword, TestQueryBuilder testQueryBuilder) : base(testQueryBuilder)
            {
                StatementKeyword = statementKeyword;
            }
        }
    }
}