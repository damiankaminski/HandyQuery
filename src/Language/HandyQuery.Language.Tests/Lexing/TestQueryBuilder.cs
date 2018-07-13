using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing;
using HandyQuery.Language.Lexing.Tokens;
using HandyQuery.Language.Lexing.Tokens.Abstract;

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
                var variants = new BuildVariants();
                var tokens = new TokenList();

                foreach (var part in _queryParts)
                {
                    switch (part)
                    {
                        case ColumnPart column:
                        {
                            variants.AddTokenToAll(column.ColumnName, 
                                index => new TextLiteralToken(index, column.ColumnName.Length, column.ColumnName));
                            break;
                        }

                        case StatementPart statement:
                        {
                            variants.AppendToAll(" ");

                            var keyword = languageConfig.Syntax.KeywordsMap[statement.StatementKeyword];
                            variants.AddTokenToAll(keyword, 
                                index => new KeywordToken(index, keyword.Length, statement.StatementKeyword));
                            break;
                        }

                        case CompareOperatorPart compareOperator:
                        {
                            var keyword = languageConfig.Syntax.KeywordsMap[compareOperator.CompareOperatorKeyword];
                            
                            // TODO: make it simpler?
                            // maybe: variants.AppendToAllExceptNewVariant(" ", () => char.IsLetterOrDigit(keyword.First()) == false)
                            BuildVariants.Variant noSpaceBeforeVariant = null;
                            if (char.IsLetterOrDigit(keyword.First()) == false)
                            {
                                // produce variant without whitespace if comp op starts with special char
                                noSpaceBeforeVariant = variants.AddNewVariant();
                            }
                            variants.AppendToAll(" ");
                            noSpaceBeforeVariant?.ReleaseToMainPool();
                            
                            variants.AddTokenToAll(keyword, 
                                index => new KeywordToken(index, keyword.Length, compareOperator.CompareOperatorKeyword));

                            BuildVariants.Variant noSpaceAfterVariant = null;
                            if (char.IsLetterOrDigit(keyword.First()) == false)
                            {
                                // produce variant without whitespace if comp op ends with special char
                                noSpaceAfterVariant = variants.AddNewVariant();
                            }
                            variants.AppendToAll(" ");
                            noSpaceAfterVariant?.ReleaseToMainPool();

                            string operand;
                            TokenBase token;

                            switch (compareOperator.Value)
                            {
                                case string s:
                                    operand = $"\"{s}\""; // TODO: produce variant without quote if single word
                                    variants.AddTokenToAll(operand, 
                                        index => new TextLiteralToken(index, operand.Length, s));
                                    break;
                                case float f:
                                    // TODO: use correct separator, as in lang config
                                    operand = f.ToString(CultureInfo.InvariantCulture); 
                                    variants.AddTokenToAll(operand, 
                                        index => new NumberLiteralToken(index, operand.Length, f));
                                    break;
                                default:
                                    throw new NotSupportedException();
                                // TODO: all  other cases
                            }

                            break;
                        }
                    }
                }

                return variants.CreateBuildResults();
            }
        }

        public class BuildVariants
        {
            private readonly List<Variant> _variants;

            public BuildVariants()
            {
                _variants = new List<Variant> {new Variant(this)};
            }

            public void AppendToAll(string queryPart)
            {
                foreach (var variant in _variants)
                {
                    variant.ExtendVariant(queryPart);
                }
            }
            
            public void AddTokenToAll(string queryPart, Func<int, TokenBase> createToken)
            {
                foreach (var variant in _variants)
                {
                    variant.AddToken(queryPart, createToken);
                }
            }

            public Variant AddNewVariant()
            {
                return new Variant(this);
            }

            public IEnumerable<BuildResult> CreateBuildResults()
            {
                return _variants.Select(x => new BuildResult(x.Query, x.ExpectedTokens)).ToList();
            }
            
            public class Variant
            {
                private readonly BuildVariants _mainPool;
                public string Query { get; private set; }
                public TokenList ExpectedTokens { get; } = new TokenList();

                public Variant(BuildVariants mainPool)
                {
                    _mainPool = mainPool;
                    
                    var baseVariant = mainPool._variants?.FirstOrDefault();
                    Query = baseVariant?.Query ?? string.Empty;
                    // TODO: copy tokens from baseVariant!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // TODO: or maybe save factory functions instead and use them on demand?
                }

                public void ExtendVariant(string queryPart)
                {
                    Query += queryPart;
                }

                public void AddToken(string queryPart, Func<int, TokenBase> createToken)
                {
                    ExpectedTokens.Add(createToken(Query.Length));
                    ExtendVariant(queryPart);
                }

                public void ReleaseToMainPool()
                {
                    _mainPool._variants.Add(this);
                }
            }
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

            public StatementPart IsEmpty() => Build(StatementKeyword.IsEmpty);
            public StatementPart IsNotEmpty() => Build(StatementKeyword.IsNotEmpty);
            public StatementPart IsTrue() => Build(StatementKeyword.IsTrue);
            public StatementPart IsFalse() => Build(StatementKeyword.IsFalse);

            // TODO: all operators
            // TODO: all value types
            public CompareOperatorPart Equal(string value) => Build(CompareOperatorKeyword.Equal, value);

            public CompareOperatorPart Equal(float value) => Build(CompareOperatorKeyword.Equal, value);
//            public CompareOperatorPart NotEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.NotEqual);
//            public CompareOperatorPart GreaterThan() => BuildCompareOperatorPart(CompareOperatorKeyword.GreaterThan);
//            public CompareOperatorPart LessThan() => BuildCompareOperatorPart(CompareOperatorKeyword.LessThan);
//            public CompareOperatorPart GreaterThanOrEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.GreaterThanOrEqual);
//            public CompareOperatorPart LessThanOrEqual() => BuildCompareOperatorPart(CompareOperatorKeyword.LessThanOrEqual);
//            public CompareOperatorPart StartsWith() => BuildCompareOperatorPart(CompareOperatorKeyword.StartsWith);
//            public CompareOperatorPart EndsWith() => BuildCompareOperatorPart(CompareOperatorKeyword.EndsWith);
//            public CompareOperatorPart Contains() => BuildCompareOperatorPart(CompareOperatorKeyword.Contains);

            private StatementPart Build(StatementKeyword statementKeyword)
            {
                var part = new StatementPart(statementKeyword, TestQueryBuilder);
                TestQueryBuilder._queryParts.Add(part);
                return part;
            }

            private CompareOperatorPart Build(CompareOperatorKeyword compareOperatorKeyword,
                object value)
            {
                var part = new CompareOperatorPart(compareOperatorKeyword, value, TestQueryBuilder);
                TestQueryBuilder._queryParts.Add(part);
                return part;
            }
        }

        public class StatementPart : PartBase
        {
            public readonly StatementKeyword StatementKeyword;

            public StatementPart(StatementKeyword statementKeyword, TestQueryBuilder testQueryBuilder) : base(
                testQueryBuilder)
            {
                StatementKeyword = statementKeyword;
            }
        }

        public class CompareOperatorPart : PartBase
        {
            public readonly CompareOperatorKeyword CompareOperatorKeyword;
            public readonly object Value;

            public CompareOperatorPart(CompareOperatorKeyword compareOperatorKeyword, object value,
                TestQueryBuilder testQueryBuilder) : base(testQueryBuilder)
            {
                CompareOperatorKeyword = compareOperatorKeyword;
                Value = value;
            }
        }
    }
}