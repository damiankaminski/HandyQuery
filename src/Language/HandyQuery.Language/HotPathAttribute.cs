using System;

namespace HandyQuery.Language
{
    /// <summary>
    /// Marks hot path. Code marked with this attribute should run as fast as possible, e.g. by avoiding
    /// heap allocations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HotPathAttribute : Attribute
    {
    }
}