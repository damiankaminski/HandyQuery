using System;

namespace HandyQuery.Language
{
    public class QueryLanguageException : Exception
    {
        public QueryLanguageException(string message) : base(message)
        {
        }
    }
}