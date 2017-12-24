using System;

namespace HandyQuery.Language
{
    internal sealed class HandyQueryInitializationException : QueryLanguageException
    {
        public HandyQueryInitializationException(string message) : base(message)
        {
        }
    }
}