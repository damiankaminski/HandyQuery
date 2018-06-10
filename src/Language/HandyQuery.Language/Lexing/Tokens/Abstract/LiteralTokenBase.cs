using System;

namespace HandyQuery.Language.Lexing.Tokens.Abstract
{
    internal abstract class LiteralTokenBase<T> : TokenBase, IEquatable<LiteralTokenBase<T>>
    {
        public readonly T Value;

        public LiteralTokenBase(int startPosition, int length, T value) : base(startPosition, length)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(LiteralTokenBase<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LiteralTokenBase<T> token && Equals(token);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}