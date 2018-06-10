using System;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing.Tokens.Abstract
{
    internal abstract class TokenBase : IEquatable<TokenBase>
    {
        public abstract TokenType TokenType { get; }
        public readonly int StartPosition;
        public readonly int Length;

        protected TokenBase(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }

        public bool Equals(TokenBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.TokenType == TokenType && StartPosition == other.StartPosition 
                && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TokenBase) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StartPosition;
                hashCode = (hashCode * 397) ^ Length;
                hashCode = (hashCode * 397) ^ (int)TokenType;
                return hashCode;
            }
        }
    }
}