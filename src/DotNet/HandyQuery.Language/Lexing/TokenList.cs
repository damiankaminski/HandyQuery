using System.Collections.Generic;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing
{
    internal sealed class TokenList : List<TokenBase>
    {
        public bool IsSameAs(TokenList other)
        {
            if (Count != other?.Count) return false;

            for (var i = 0; i < Count; i++)
            {
                if (this[i].Equals(other[i]) == false)
                {
                    return false;
                }                
            }

            return true;
        }
    }
}