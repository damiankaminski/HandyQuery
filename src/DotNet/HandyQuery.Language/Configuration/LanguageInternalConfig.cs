using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HandyQuery.Language.Configuration
{
    internal interface ILanguageInternalConfig
    {
        IReadOnlyList<ColumnInfo> Columns { get; }
        SyntaxInfo SyntaxInfo { get; }
        Type ModelType { get; }
        IReadOnlyDictionary<CultureInfo, CultureConfig> Cultures { get; }

        /// <summary>
        /// Gets column by name. Uses <see cref="ISyntaxConfig.ColumnNameCaseSensitive"/> configuration.
        /// </summary>
        ColumnInfo GetColumnInfo(string columnName); // TODO: create ColumnList and move it there

        ICultureConfig GetCulture(CultureInfo culture);
    }

    internal sealed class LanguageInternalConfig : ILanguageInternalConfig
    {
        public readonly List<ColumnInfo> UnderlyingColumns = new List<ColumnInfo>();
        public SyntaxInfo SyntaxInfo { get; } = new SyntaxInfo(new SyntaxConfig());
        public readonly Dictionary<CultureInfo, CultureConfig> UnderlyingCultures = new Dictionary<CultureInfo, CultureConfig>();
        public Type ModelType { get; }

        IReadOnlyList<ColumnInfo> ILanguageInternalConfig.Columns => UnderlyingColumns;
        IReadOnlyDictionary<CultureInfo, CultureConfig> ILanguageInternalConfig.Cultures => UnderlyingCultures;

        private readonly CultureConfig _defaultCulture;

        public LanguageInternalConfig(Type modelType)
        {
            ModelType = modelType;
            _defaultCulture = new CultureConfig(CultureInfo.GetCultureInfo("en-us"));
        }

        public ColumnInfo GetColumnInfo(string columnName)
        {
            if (SyntaxInfo.Config.ColumnNameCaseSensitive)
                return UnderlyingColumns.FirstOrDefault(x => x.ColumnName == columnName);

            return UnderlyingColumns.FirstOrDefault(x => x.ColumnName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));
        }

        public ICultureConfig GetCulture(CultureInfo culture)
        {
            CultureConfig cultureConfig;
            if (UnderlyingCultures.TryGetValue(culture, out cultureConfig) == false)
            {
                return _defaultCulture;
            }

            return cultureConfig;
        }
    }
}