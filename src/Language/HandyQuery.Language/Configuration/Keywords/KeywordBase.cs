﻿namespace HandyQuery.Language.Configuration.Keywords
{
    internal abstract class KeywordBase
    {
        public abstract KeywordType KeywordType { get; }

        public override string ToString()
        {
            return KeywordType.ToString();
        }
    }
}