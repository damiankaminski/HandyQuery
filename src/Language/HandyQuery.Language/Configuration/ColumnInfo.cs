using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Configuration
{
    /// <summary>
    /// Provides basic info about column.
    /// </summary>
    internal sealed class ColumnInfo : IEquatable<ColumnInfo>
    {
        /// <summary>
        /// Name of the column in provided query.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Underlying member name in given .NET type.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Underlying .NET type.
        /// </summary>
        public Type SystemType { get; }

        /// <summary>
        /// Is <see cref="SystemType"/> nullable.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        /// HQL column type.
        /// </summary>
        public HandyType HandyType { get; }

        private static readonly Dictionary<HandyType, KeywordBase[]> AllowedColumnKeywords = new Dictionary<HandyType, KeywordBase[]>
        {
            {
                HandyType.DateTime,
                new KeywordBase[]
                {
                    StatementKeyword.IsEmpty, StatementKeyword.IsNotEmpty,
                    CompareOperatorKeyword.Equal, CompareOperatorKeyword.NotEqual,
                    CompareOperatorKeyword.GreaterThan, CompareOperatorKeyword.GreaterThanOrEqual,
                    CompareOperatorKeyword.LessThan, CompareOperatorKeyword.LessThanOrEqual
                }
            },
            {
                HandyType.Float,
                new KeywordBase[]
                {
                    StatementKeyword.IsEmpty, StatementKeyword.IsNotEmpty,
                    CompareOperatorKeyword.Equal, CompareOperatorKeyword.NotEqual,
                    CompareOperatorKeyword.GreaterThan, CompareOperatorKeyword.GreaterThanOrEqual,
                    CompareOperatorKeyword.LessThan, CompareOperatorKeyword.LessThanOrEqual
                }
            },
            {
                HandyType.Integer,
                new KeywordBase[]
                {
                    StatementKeyword.IsEmpty, StatementKeyword.IsNotEmpty,
                    CompareOperatorKeyword.Equal, CompareOperatorKeyword.NotEqual,
                    CompareOperatorKeyword.GreaterThan, CompareOperatorKeyword.GreaterThanOrEqual,
                    CompareOperatorKeyword.LessThan, CompareOperatorKeyword.LessThanOrEqual
                }
            },
            {
                HandyType.String,
                new KeywordBase[]
                {
                    StatementKeyword.IsEmpty, StatementKeyword.IsNotEmpty,
                    CompareOperatorKeyword.Equal, CompareOperatorKeyword.NotEqual,
                    CompareOperatorKeyword.Contains,
                    CompareOperatorKeyword.StartsWith, CompareOperatorKeyword.EndsWith
                }
            },
            {
                HandyType.Boolean,
                new KeywordBase[]
                {
                    StatementKeyword.IsEmpty, StatementKeyword.IsNotEmpty,
                    StatementKeyword.IsTrue, StatementKeyword.IsFalse
                }
            }
        };

        public ColumnInfo(string columnName, string memberName, Type systemType)
        {
            ColumnName = columnName;
            MemberName = memberName;
            SystemType = systemType;
            IsNullable = systemType.IsNullable();

            // todo: create a unit test for finding ColumnType property
            var columnTypeFound = false;
            foreach (var column in Enum.GetValues(typeof(HandyType)))
            {
                if (columnTypeFound)
                    break;

                var attr = typeof(HandyType).GetTypeInfo()
                    .GetDeclaredField(column.ToString())
                    .GetCustomAttribute<SystemTypesAttribute>();

                if (attr.Types.Any(type => type == systemType))
                {
                    HandyType = (HandyType)column;
                    columnTypeFound = true;
                }
            }

            if (columnTypeFound == false)
            {
                var msg = $"Column '{columnName}' has unsupported type.";
                throw new ConfigurationException(msg, ConfigurationExceptionType.UnsupportedColumnNameType);
            }
        }

        /// <summary>
        /// Checks if given compare operator is allowed for this column type.
        /// </summary>
        /// <returns>True if allowed; false otherwise.</returns>
        internal bool IsAllowed(CompareOperatorKeyword compareOperatorKeyword)
        {
            return AllowedColumnKeywords[HandyType].Contains(compareOperatorKeyword);
        }

        /// <summary>
        /// Checks if given statement is allowed for this column type.
        /// </summary>
        /// <returns>True if allowed; false otherwise.</returns>
        internal bool IsAllowed(StatementKeyword statementKeyword)
        {
            switch (statementKeyword.StatementType)
            {
                case StatementType.IsEmpty:
                case StatementType.IsNotEmpty:
                    return SystemType.IsNullable();

                case StatementType.IsFalse:
                case StatementType.IsTrue:
                    return SystemType == typeof(bool) || SystemType == typeof(bool?);
            }

            return false;
        }

        public bool Equals(ColumnInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ColumnName, other.ColumnName) && string.Equals(MemberName, other.MemberName) 
                && SystemType == other.SystemType && IsNullable == other.IsNullable && HandyType == other.HandyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ColumnInfo && Equals((ColumnInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ColumnName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (MemberName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SystemType != null ? SystemType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsNullable.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) HandyType;
                return hashCode;
            }
        }
    }
}