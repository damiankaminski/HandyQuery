using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaPartBody : List<IGrammaBodyItem>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammaPartBody && Equals((GrammaPartBody)obj);
        }

        public bool Equals(GrammaPartBody body)
        {
            return this.IsSameAs(body);
        }

        public override string ToString()
        {
            return $"{String.Join(" ", this.Select(x => x.ToString()))}";
        }
    }
}