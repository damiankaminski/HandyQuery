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
            var columnName = propertyOrField.GetFullPropertyName();
            return AddColumn(columnName, columnName);
        }

        /// <summary>
        /// Adds new column with custom name.
        /// </summary>
        public ConfigurationBuilder<T> AddColumn(string columnName, Expression<Func<T, object>> propertyOrField)
        {
            return AddColumn(columnName, propertyOrField.GetFullPropertyName());
        }

        private ConfigurationBuilder<T> AddColumn(string columnName, string accessor)
        {
            var tmpConfig = CreateConfig();
            if (tmpConfig.GetColumnInfo(columnName) != null)
                throw new ConfigurationException($"Column named as '${columnName}' is defined twice.");

            // TODO: support for fields
            var property = typeof(T).GetNestedProperty(accessor);

            if (property == null)
                throw new ConfigurationException("Invalid accessor. Needs to return a field or property and do " +
                                                 $"nothing else. Currently defined as: \n{accessor}");

            var column = new ColumnInfo(columnName, accessor, property.PropertyType);

            ValidateColumnName(column);

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
            // TODO: test
            var reservedChars = _syntax.ReservedChars.ToArray();
            var columnName = column.ColumnName;
            var invalidIndex = columnName.IndexOfAny(reservedChars);
            if (invalidIndex >= 0)
            {
                var msg = $"Column name ('{columnName}') contains invalid character: {reservedChars[invalidIndex]}.";
                throw new ConfigurationException(msg);
            }
        }
    }
}