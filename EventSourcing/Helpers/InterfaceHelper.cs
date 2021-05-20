using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventSourcing.Helpers
{
    public static class InterfaceHelper
    {
        public static bool IsImplementAny(this Type type, params Type[] interfaces) => 
            interfaces.Any(i => type.GetAllInterfaces().Contains(i));


        public static bool IsImplementAll(this Type type, params Type[] interfaces) =>
            !interfaces.Any(i => !type.GetAllInterfaces().Contains(i));

        public static IEnumerable<Type> GetAllInterfaces(this Type type) =>
            type.GetInterfaces().Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i);
    }
}
