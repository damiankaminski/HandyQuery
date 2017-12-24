using System.Collections.Generic;
using HandyQuery.Language.Configuration.Keywords;

namespace HandyQuery.Language.Configuration
{
    internal interface ICultureConfig
    {
        /// <summary>
        /// Date formats.
        /// </summary>
        IEnumerable<string> DateTimeFormats { get; }

        /// <summary>
        /// A constant which represents a null value.
        /// </summary>
        string NullConstant { get; }

        /// <summary>
        /// Number decimal separator. Used while parsing decimal numbers.
        /// </summary>
        char NumberDecimalSeparator { get; }

        /// <summary>
        /// Keywords translations.
        /// </summary>
        IReadOnlyDictionary<Keyword, string> Keywords { get; }
    }
}