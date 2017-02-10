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

        public bool IsOptional { get; } = false;
        public GrammarElementType Type => GrammarElementType.OrCondition;

        public GrammarOrCondition(List<IGrammarBodyItem> operands)
        {
            Operands = operands;
        }

        public override string ToString()
        {
            return $"{String.Join("|", Operands.Select(x => x.ToString()))}";
        }
    }
}