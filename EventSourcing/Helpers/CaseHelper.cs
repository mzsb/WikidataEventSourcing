using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventSourcing.Helpers
{
    public static class CaseHelper
    {
        public static string ToCamelCase(this string str) => str?.ToCase(char.ToLower);

        public static string ToPascalCase(this string str) => str?.ToCase(char.ToUpper);

        private static string ToCase(this string str, Func<char, char> caseFunction) => string.IsNullOrEmpty(str) ?
            throw new Exception("Invalid string to case") :
            caseFunction(str[0]) + (str.Length > 1 ? str[1..] : string.Empty);

        public static string GetId(this string jsonEntity)
        {
            var splitted = jsonEntity.ToLower().Split('"');
            return splitted[Array.IndexOf(splitted, "id") + 2];
        }
    }
}
