using System;
using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SystemTypesAttribute : Attribute
    {
        public List<Type> Types { get; private set; }

        public SystemTypesAttribute(params Type[] types)
        {
            Types = types.ToList();
        }
    }
}