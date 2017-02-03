using System;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Lexing.Tokens.Abstract
{
    internal abstract class KeywordTokenBase : TokenBase, IEquatable<KeywordTokenBase>
    {
        public readonly Keyword Keyword;

        protected KeywordTokenBase(int startPosition, int length, Keyword keyword) : base(startPosition, length)
        {
            Keyword = keyword;
        }

        public override string ToString()
        {
            return Keyword.TokenType.ToString();
        }

        public bool Equals(KeywordTokenBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Keyword, other.Keyword);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeywordTokenBase) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Keyword != null ? Keyword.GetHashCode() : 0);
            }
        }
    }
}