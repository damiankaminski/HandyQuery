using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarPartBody : List<IGrammarBodyItem>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarPartBody && Equals((GrammarPartBody)obj);
        }

        public bool Equals(GrammarPartBody body)
        {
            return this.IsSameAs(body);
        }

        public override string ToString()
        {
            return $"{String.Join(" ", this.Select(x => x.ToString()))}";
        }
    }
}