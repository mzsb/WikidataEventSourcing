using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventSourcing.Helpers
{
    public static class TypeHelper
    {
        public static bool IsPrimitive(this Type type) =>
            type.IsPrimitive ||
            new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(type) ||
            type.IsEnum ||
            Convert.GetTypeCode(type) != TypeCode.Object ||
            (type.IsGenericType && 
             type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
             type.GetGenericArguments()[0].IsPrimitive());

        public static bool IsSystem(this Type type) =>
            type.Namespace.StartsWith("System");

        public static List<Type> GetBases(this Type type, bool withSelf = false)
        {
            List<Type> bases = withSelf ? new() { type } : new();
            var @base = type.BaseType;
            while (@base is not null)
            {
                bases.Add(@base);
                @base = @base.BaseType;
            }
            return bases;
        }
    }
}
