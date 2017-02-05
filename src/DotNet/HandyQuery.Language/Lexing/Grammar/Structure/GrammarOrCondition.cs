using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Extensions;

namespace HandyQuery.Language.Lexing.Grammar.Structure
{
    internal sealed class GrammarOrCondition : IGrammarBodyItem
    {
        public string Name => null;
        public List<IGrammarBodyItem> Operands { get; }

        public GrammarElementType Type => GrammarElementType.OrCondition;

        public GrammarOrCondition(List<IGrammarBodyItem> operands)
        {
            Operands = operands;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GrammarOrCondition && Equals((GrammarOrCondition)obj);
        }

        public bool Equals(GrammarOrCondition or)
        {
            return Operands.IsSameAs(or.Operands);
        }

        public override string ToString()
        {
            return $"{String.Join("|", Operands.Select(x => x.ToString()))}";
        }
    }
}