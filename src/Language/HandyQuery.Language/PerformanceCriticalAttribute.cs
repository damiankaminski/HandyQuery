using System;

namespace HandyQuery.Language
{
    // TODO: create diagnose which will go through performance critical code
    // TODO: for a start, search for HeapAllocAttribute
    
    /// <summary>
    /// Marks language hot path. Code marked with this attribute should run as fast as possibly, e.g. by avoid
    /// heap allocations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PerformanceCriticalAttribute : Attribute
    {
    }
}