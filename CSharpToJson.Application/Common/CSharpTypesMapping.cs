using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Common
{
    internal static class CSharpTypesMapping
    {
        private static readonly Dictionary<string, string> Mappings = new()
        {
            { "string", "\"\"" },
            { "String", "\"\"" },

            { "bool", "true" },
            { "bool?", "true" },
            { "Boolean", "true" },
            { "Boolean?", "true" },

            { "Guid", "Guid.NewGuid()"},

            { "Datetime", "DateTime.Now"},
            { "Datetime?", "DateTime.Now"},
            { "DateTime", "DateTime.Now"},
            { "DateTime?", "DateTime.Now"},

            { "DateTimeOffset", "DateTimeOffset.Now"},
            { "DateTimeOffset?", "DateTimeOffset.Now"},

            { "int", "1"},
            { "int?", "1"},

            { "uint", "1"},
            { "long", "1"},
            { "double", "1"},
            { "Double", "1"},
            { "float", "1"},
            { "decimal", "1"},
            { "byte", "1"},

            { "char", "\'c\'"},

        };

        internal static string Map(string type)
        {
            return Mappings[type];
        }

        internal static string MapArray(ObjectModel type)
        {
            return $"new {type.GenericType}[]{{{Mappings[type.GenericType]}}}";
        }

        internal static string MapObject(ObjectModel type)
        {
            return type.PropertyType.Contains("IEnumerable") ?
                $"new {type.PropertyType.Replace("IEnumerable", "List")}()" :
                $"new {type.PropertyType}()";
        }
    }
}
