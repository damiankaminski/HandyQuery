using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaPartBody : List<IGrammaBodyItem>
    {
        public override string ToString()
        {
            return $"{String.Join(" ", this.Select(x => x.ToString()))}";
        }
    }
}