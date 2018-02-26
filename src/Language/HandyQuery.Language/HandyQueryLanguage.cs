using HandyQuery.Language.Configuration;

namespace HandyQuery.Language
{
    public class HandyQueryLanguage
    {
        public static SyntaxBuilder BuildSyntax()
        {
            return new SyntaxBuilder();
        }

        public static ConfigurationBuilder<T> Configure<T>() where T : class
        {
            return new ConfigurationBuilder<T>(BuildSyntax());
        }
        
        public static ConfigurationBuilder<T> Configure<T>(SyntaxBuilder syntaxBuilder) where T : class
        {
            return new ConfigurationBuilder<T>(syntaxBuilder);
        }
    }
}