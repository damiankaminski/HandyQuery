using System;
using System.Linq;
using System.Linq.Expressions;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    public sealed class LanguageConfig<T>
    {
        private readonly LanguageInternalConfig _config;

        internal ILanguageInternalConfig InternalConfig => _config;

        public LanguageConfig()
        {
            _config = new LanguageInternalConfig(typeof(T));
        }

        /// <summary>
        /// Configures language syntax.
        /// </summary>
        public LanguageConfig<T> Syntax(Action<SyntaxConfig> config)
        {
            config((SyntaxConfig)_config.SyntaxInfo.Config);

            _config.SyntaxInfo.RecalculateReservedChars();

            // since syntax has been changed then compatibility needs to be validated
            ValidateSyntaxCompatibility();
            return this;
        }

        /// <summary>
        /// Adds new column.
        /// </summary>
        /// <param name="columnName">Name of the column which will be available in the query.</param>
        /// <param name="property">Property in the model.</param>
        public LanguageConfig<T> AddColumn(string columnName, Expression<Func<T, object>> property)
        {
            return AddColumn(columnName, property.GetFullPropertyName());
        }

        /// <summary>
        /// Adds new column.
        /// </summary>
        /// <param name="property">Property in the model.</param>
        public LanguageConfig<T> AddColumn(Expression<Func<T, object>> property)
        {
            var columnName = property.GetFullPropertyName();
            return AddColumn(columnName, columnName);
        }

        /// <summary>
        /// Adds new column.
        /// </summary>
        /// <param name="columnName">Name of the column which will be available in the query.</param>
        /// <param name="propertyName">Name of the property in the model.</param>
        /// <exception cref="ArgumentException">When propertyName or columnName is invalid due to some restrictions.</exception>
        public LanguageConfig<T> AddColumn(string columnName, string propertyName)
        {
            if (_config.GetColumnInfo(columnName) != null)
                throw new ArgumentException("Column with given name already exist.", nameof(columnName));

            // TODO: support for fields
            var property = _config.ModelType.GetNestedProperty(propertyName);

            if (property == null)
                throw new ArgumentException("Given property name does not exist.", nameof(propertyName));

            var column = new ColumnInfo(columnName, propertyName, property.PropertyType);

            ValidateColumnName(column);

            _config.UnderlyingColumns.Add(column);
            return this;
        }

        /// <summary>
        /// Checks if syntax configuration is compatible with other options.
        /// </summary>
        private void ValidateSyntaxCompatibility()
        {
            foreach (var column in _config.UnderlyingColumns)
            {
                ValidateColumnName(column);
            }
        }

        private void ValidateColumnName(ColumnInfo column)
        {
            var reservedChars = _config.SyntaxInfo.ReservedChars.ToArray();
            var columnName = column.ColumnName;
            if (columnName.IndexOfAny(reservedChars) > 0)
            {
                var invalidChars = string.Join(", ", reservedChars);
                var msg = $"Given column name ('{columnName}') contains one of the invalid chars: {invalidChars}.";
                throw new ArgumentException(msg, nameof(columnName));
            }
        }
    }
}