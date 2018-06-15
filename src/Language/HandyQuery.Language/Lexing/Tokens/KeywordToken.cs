using System;
using HandyQuery.Language.Configuration.Keywords;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing.Tokens
{
    internal sealed class KeywordToken : TokenBase, IEquatable<KeywordToken>
    {
        public override TokenType TokenType => TokenType.Keyword;

        public readonly KeywordBase Keyword;
        
        public KeywordToken(int startPosition, int length, KeywordBase keyword) : base(startPosition, length)
        {
            Keyword = keyword;
        }

        public bool Equals(KeywordToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Keyword, other.Keyword);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is KeywordToken && Equals((KeywordToken) obj);
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