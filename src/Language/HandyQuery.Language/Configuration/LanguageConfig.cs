using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    internal sealed class LanguageConfig
    {
        public readonly Type ModelType;
        public readonly IReadOnlyList<ColumnInfo> Columns;
        public readonly SyntaxConfig SyntaxConfig;

        public LanguageConfig(Type modelType, IReadOnlyList<ColumnInfo> columns, SyntaxConfig syntaxConfig)
        {
            ModelType = modelType;
            Columns = columns;
            SyntaxConfig = syntaxConfig;
        }
        
        public ColumnInfo GetColumnInfo(string columnName)
        {
            // TODO: get rid of lambda allocations
            
            if (SyntaxConfig.ColumnNameCaseSensitive)
                return Columns.FirstOrDefault(x => x.ColumnName == columnName);

            return Columns.FirstOrDefault(x => x.ColumnName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}