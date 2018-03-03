namespace HandyQuery.Language.Configuration
{
    public class ConfigurationException : QueryLanguageException
    {
        public readonly ConfigurationExceptionType ExceptionType; 
        
        public ConfigurationException(string message, ConfigurationExceptionType exceptionType) : base(message)
        {
            ExceptionType = exceptionType;
        }
    }

    public enum ConfigurationExceptionType
    {
        InvalidColumnName,
        InvalidColumnNameMemberDefinition,
        DuplicatedColumnName,
        UnsupportedColumnNameType
    }
}