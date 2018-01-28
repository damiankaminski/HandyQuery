using System;

namespace HandyQuery.Language
{
    // TODO: create diagnose which will go through performance critical code
    // TODO: for a start, search for HeapAllocAttribute
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PerformanceCriticalAttribute : Attribute
    {
    }
}