using System;
using System.Collections.Generic;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar.Structure;
using HandyQuery.Language.Lexing.Tokenizers.Abstract;

namespace HandyQuery.Language.Lexing.Graph.Builder
{
    // TODO: get rid of not used methods

    internal sealed class BuilderNode
    {
        public readonly ITokenizer Tokenizer;
        public readonly bool IsOptional;
        public readonly HashSet<BuilderNode> Children = new HashSet<BuilderNode>();
        public readonly HashSet<BuilderNode> Parents = new HashSet<BuilderNode>();

        public BuilderNode(GrammarTokenizerUsage tokenizerUsage)
        {
            Tokenizer = tokenizerUsage?.Impl;
            IsOptional = tokenizerUsage?.IsOptional ?? false;
        }

        public void AddParents(IEnumerable<BuilderNode> parents)
        {
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent?.AddChild(this);
                }
            }
        }

        public BuilderNode AddChild(BuilderNode child)
        {
            Children.Add(child);
            child.AddParent(this);
            return this;
        }

        private BuilderNode AddChildren(BuilderNode[] children)
        {
            foreach (var child in children)
            {
                Children.Add(child);
                child.AddParent(this);
            }
            return this;
        }

        private void AddParent(BuilderNode parent)
        {
            Parents.Add(parent);
        }

        /// <summary>
        /// Finds first non optional parent in all parent branches (single node may have multiple parents).
        /// </summary>
        public IEnumerable<BuilderNode> FindFirstNonOptionalParentInAllParentBranches()
        {
            foreach (var parent in Parents.ToArray())
            {
                if (parent.IsOptional == false)
                {
                    yield return parent;
                    continue;
                }

                foreach (var nonOptionalParent in parent.FindFirstNonOptionalParentInAllParentBranches().ToArray())
                {
                    yield return nonOptionalParent;
                }
            }
        }

        /// <summary>
        /// Finds first non optional child in all child branches (single node may have multiple children).
        /// </summary>
        public IEnumerable<BuilderNode> FindFirstNonOptionalChildInAllChildBranches()
        {
            foreach (var child in Children.ToArray())
            {
                if (child.IsOptional == false)
                {
                    yield return child;
                    continue;
                }

                foreach (var nonOptionalChild in child.FindFirstNonOptionalChildInAllChildBranches().ToArray())
                {
                    yield return nonOptionalChild;
                }
            }
        }

        public bool Equals(BuilderNode node, HashSet<BuilderNode> visitedNodes = null)
        {
            if (node.Tokenizer?.GetType() != Tokenizer?.GetType())
            {
                return false;
            }

            if (node.IsOptional != IsOptional)
            {
                return false;
            }

            visitedNodes = visitedNodes ?? new HashSet<BuilderNode>();
            if (visitedNodes.Contains(node)) return true;
            visitedNodes.Add(node);

            if (AreSame(node.Children, Children, visitedNodes) == false)
            {
                return false;
            }

            return true;
        }

        private static bool AreSame(IEnumerable<BuilderNode> items, IEnumerable<BuilderNode> items2, HashSet<BuilderNode> visitedNodes)
        {
            var i1 = items.ToList();
            var i2 = items2.ToList();

            if (i1.Count != i2.Count)
            {
                return false;
            }

            for (var i = 0; i < i2.Count; i++)
            {
                if (i2[i].Equals(i1[i], visitedNodes) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var optional = IsOptional ? "?" : "";
            var name = Tokenizer?.GetType().Name;
            if (name != null)
            {
                name = name.Substring(0, name.LastIndexOf("Tokenizer", StringComparison.Ordinal));
            }
            else
            {
                name = "ROOT";
            }
            return $"{optional}{name}";
        }
    }
}