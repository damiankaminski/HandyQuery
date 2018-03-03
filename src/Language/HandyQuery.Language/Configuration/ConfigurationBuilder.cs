using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    public sealed class ConfigurationBuilder<T> where T : class
    {
        private readonly List<ColumnInfo> _columns = new List<ColumnInfo>();

        private readonly SyntaxConfig _syntax;

        private LanguageConfig _finalConfig;

        public ConfigurationBuilder(SyntaxBuilder syntaxBuilder)
        {
            _syntax = syntaxBuilder.Build();
        }

        internal LanguageConfig Build()
        {
            if (_finalConfig != null) return _finalConfig;

            ValidateSyntaxCompatibility();
            _finalConfig = CreateConfig();

            return _finalConfig;
        }

        /// <summary>
        /// Adds new column.
        /// </summary>
        public ConfigurationBuilder<T> AddColumn(Expression<Func<T, object>> propertyOrField)
        {
            var (memberName, memberType) = AnalyzeMemberDefinition(propertyOrField);
            return AddColumn(memberName, memberName, memberType);
        }

        /// <summary>
        /// Adds new column with custom name.
        /// </summary>
        public ConfigurationBuilder<T> AddColumn(string columnName, Expression<Func<T, object>> propertyOrField)
        {
            var (memberName, memberType) = AnalyzeMemberDefinition(propertyOrField);
            return AddColumn(columnName, memberName, memberType);
        }

        private static (string MemberName, Type MemberType) AnalyzeMemberDefinition(Expression<Func<T, object>> propertyOrField)
        {
            // TODO: support for fields
            var memberName = propertyOrField.GetFullPropertyName();
            var property = typeof(T).GetNestedProperty(memberName);

            if (property == null)
                throw new ConfigurationException(
                    "Invalid column name definition. 'propertyOrField' argument needs to return a property or field " +
                    $"and nothing else. Currently defined as: {propertyOrField}",
                    ConfigurationExceptionType.InvalidColumnNameMemberDefinition);
            
            return (memberName, property.PropertyType);
        }
        
        private ConfigurationBuilder<T> AddColumn(string columnName, string memberName, Type systemType)
        {
            var tmpConfig = CreateConfig();
            if (tmpConfig.GetColumnInfo(columnName) != null)
                throw new ConfigurationException(
                    $"Column named as '{columnName}' is defined twice.",
                    ConfigurationExceptionType.DuplicatedColumnName);

            var column = new ColumnInfo(columnName, memberName, systemType);

            _columns.Add(column);
            return this;
        }

        private LanguageConfig CreateConfig()
        {
            return new LanguageConfig(typeof(T), _columns, _syntax);
        }

        /// <summary>
        /// Checks if syntax is compatible with language configuration.
        /// </summary>
        private void ValidateSyntaxCompatibility()
        {
            foreach (var column in _columns)
            {
                ValidateColumnName(column);
            }
        }

        private void ValidateColumnName(ColumnInfo column)
        {
            var reservedChars = _syntax.ReservedChars.ToArray();
            var columnName = column.ColumnName;
            var invalidIndex = columnName.IndexOfAny(reservedChars);
            if (invalidIndex >= 0)
            {
                var msg = $"Column name ('{columnName}') contains invalid character: {columnName[invalidIndex]}.";
                throw new ConfigurationException(msg, ConfigurationExceptionType.InvalidColumnName);
            }
        }
    }
}