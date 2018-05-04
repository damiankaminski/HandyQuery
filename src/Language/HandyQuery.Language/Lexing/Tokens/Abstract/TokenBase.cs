using System;
using HandyQuery.Language.Configuration;

namespace HandyQuery.Language.Lexing.Tokens.Abstract
{
    // TODO: use struct with interface and "constrained" feature to avoid boxing
    
    internal abstract class TokenBase : IEquatable<TokenBase>
    {
        /// <summary>
        /// Type of a token. Each token should have their own <see cref="TokenType"/>. 
        /// Allows to ensure token type without casting.
        /// </summary>
        public abstract TokenType TokenType { get; }

        /// <summary>
        /// Start index in the query.
        /// </summary>
        public readonly int StartPosition;

        /// <summary>
        /// Length of token in the query.
        /// </summary>
        public readonly int Length;

        protected TokenBase(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }

        public T As<T>() where T : TokenBase
        {
            return this as T;
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
            if (obj.GetType() != this.GetType()) return false;
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