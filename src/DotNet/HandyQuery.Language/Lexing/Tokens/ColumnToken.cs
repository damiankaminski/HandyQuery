using System;
using HandyQuery.Language.Configuration;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class ColumnToken : TokenBase, IEquatable<ColumnToken>
    {
        public override TokenType TokenType => TokenType.Column;

        public readonly ColumnInfo ColumnInfo;

        public ColumnToken(int startPosition, int length, ColumnInfo columnInfo) : base(startPosition, length)
        {
            ColumnInfo = columnInfo;
        }

        public override string ToString()
        {
            return ColumnInfo.ColumnName;
        }

        public bool Equals(ColumnToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(ColumnInfo, other.ColumnInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ColumnToken && Equals((ColumnToken) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ColumnInfo != null ? ColumnInfo.GetHashCode() : 0);
            }
        }
    }
}