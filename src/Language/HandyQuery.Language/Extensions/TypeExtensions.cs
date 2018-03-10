using System;
using System.Reflection;

namespace HandyQuery.Language.Extensions
{
    internal static class TypeExtensions
    {
        public static Type GetNestedType(this Type type, string fullPropertyOrFieldName)
        {
            if (type == null || string.IsNullOrWhiteSpace(fullPropertyOrFieldName))
                return null;

            var parts = fullPropertyOrFieldName.Split('.');
            foreach (var part in parts)
            {
                var field = type.GetField(part);
                if (field != null)
                {
                    type = field.FieldType;
                    continue;
                }
                
                var property = type.GetProperty(part);
                if (property != null)
                {
                    type = property.PropertyType;
                    continue;
                }
                
                return null;
            }

            return type;
        }

        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType == false)
                return true; // ref-type

            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>

            return false; // value-type
        }
    }
}