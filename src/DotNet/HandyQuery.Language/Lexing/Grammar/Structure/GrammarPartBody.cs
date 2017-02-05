using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarPartBody : List<IGrammarBodyItem>
    {
        public override string ToString()
        {
            return $"{String.Join(" ", this.Select(x => x.ToString()))}";
        }
    }
}