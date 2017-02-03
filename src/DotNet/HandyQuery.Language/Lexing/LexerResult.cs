using System.Collections.Generic;
using HandyQuery.Language.Lexing.Tokens.Abstract;

namespace HandyQuery.Language.Lexing
{
    internal sealed class LexerResult
    {
        public LexerResult()
        {
            Errors = new ErrorList(); // TODO: pool
            Tokens = new TokenList(); // TODO: pool
        }

        /// <summary>
        /// List of errors which occurred while tokenizing.
        /// </summary>
        public ErrorList Errors { get; private set; }

        /// <summary>
        /// List of successfully created tokens.
        /// </summary>
        public TokenList Tokens { get; private set; }
    }
}