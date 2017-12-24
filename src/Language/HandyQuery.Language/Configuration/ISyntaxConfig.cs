using System.Collections.Generic;

namespace HandyQuery.Language.Configuration
{
    /// <summary>
    /// Configuration of syntax.
    /// </summary>
    /// <remarks>
    /// The purpose of this interface is to make <see cref="SyntaxConfig"/> immutable in most places.
    /// </remarks>
    public interface ISyntaxConfig
    {
        /// <summary>
        /// If true then column searching will be case sensitive.
        /// </summary>
        bool ColumnNameCaseSensitive { get; }

        /// <summary>
        /// If true then all keywords will be paresed using case sensitive comparison.
        /// </summary>
        bool KeywordCaseSensitive { get; }

        /// <summary>
        /// Allows to configure if null constant is parsed with case sensivity.
        /// </summary>
        bool NullConstantCaseSensitive { get; }

        /// <summary>
        /// Open parenthesis char.
        /// </summary>
        char ParenOpen { get; }

        /// <summary>
        /// Close parenthesis char.
        /// </summary>
        char ParenClose { get; }

        /// <summary>
        /// Used for parsing strings. Each string has to start and end with this char.
        /// </summary>
        char StringLiteralIdentifier { get; }

        /// <summary>
        /// Start of parameters in function invokation.
        /// </summary>
        char ParamsOpen { get; }

        /// <summary>
        /// End of parameters in function invokation.
        /// </summary>
        char ParamsClose { get; }
    }
}