using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammaOrCondition && Equals((GrammaOrCondition)obj);
        }

        public bool Equals(GrammaOrCondition or)
        {
            return Operands.IsSameAs(or.Operands);
        }

        public override string ToString()
        {
            return $"{String.Join("|", Operands.Select(x => x.ToString()))}";
        }
    }
}