using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language.Lexing.Gramma.Structure
{
    internal sealed class GrammaOrCondition : IGrammaBodyItem
    {
        public string Name => null;
        public List<IGrammaBodyItem> Operands { get; }

        public GrammaElementType Type => GrammaElementType.OrCondition;

        public GrammaOrCondition(List<IGrammaBodyItem> operands)
        {
            Operands = operands;
        }

        public override string ToString()
        {
            return $"{String.Join("|", Operands.Select(x => x.ToString()))}";
        }
    }
}