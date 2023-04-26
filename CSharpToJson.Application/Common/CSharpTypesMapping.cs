using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Common
{
    internal static class CSharpTypesMapping
    {
        private static readonly Dictionary<List<string>, object> Mappings = new ()
        {
            {new List<string>
            {
                "string",
                "String"
            }, "\"string\""},
            {new List<string>
            {
                "object",
                "Object"
            }, "{}"},
            {new List<string>
            {
                "bool",
                "Boolean"
            }, default(bool).ToString().ToLower()},
            {new List<string>
            {
                "Boolean?",
                "bool?",
            }, default(bool?)},
            {new List<string>
            {
                "Guid"
            }, $"\"{Guid.NewGuid()}\""},
            {new List<string>
            {
                "Guid?"
            }, default(Guid?)},
            {new List<string>
            {
                "DateTime"
            }, $"\"{default(DateTime)}\""},
            {new List<string>
            {
                "DateTime?"
            }, default(DateTime?)},
            {new List<string>
            {
                "TimeSpan"
            }, $"\"{default(TimeSpan)}\""},
            {new List<string>
            {
                "TimeSpan?"
            }, default(TimeSpan?)},
            {new List<string>
            {
                "byte",
                "Byte"
            }, default(byte)},
            {new List<string>
            {
                "byte?",
                "Byte?"
            }, default(byte?)},
            {new List<string>
            {
                "sbyte",
                "SByte"
            }, default(sbyte)},
            {new List<string>
            {
                "sbyte?",
                "SByte?"
            }, default(sbyte?)},
            {new List<string>
            {
                "short",
                "Int16"
            }, default(short)},
            {new List<string>
            {
                "short?",
                "Int16?"
            }, default(short?)},
            {new List<string>
            {
                "ushort",
                "UInt16"
            }, default(ushort)},
            {new List<string>
            {
                "ushort?",
                "UInt16?"
            }, default(ushort?)},
            {new List<string>
            {
                "int",
                "Int32"
            }, default(int)},
            {new List<string>
            {
                "int?",
                "Int32?"
            }, default(int?)},
            {new List<string>
            {
                "uint",
                "UInt32"
            }, default(uint)},
            {new List<string>
            {
                "uint?",
                "UInt32?"
            }, default(uint?)},
            {new List<string>
            {
                "long",
                "Int64"
            }, default(long)},
            {new List<string>
            {
                "long?",
                "Int64?"
            }, default(long?)},
            {new List<string>
            {
                "ulong",
                "UInt64"
            }, default(ulong)},
            {new List<string>
            {
                "ulong?",
                "UInt64?"
            }, default(ulong?)},
            {new List<string>
            {
                "double",
                "Double"
            }, default(double)},
            {new List<string>
            {
                "double?",
                "Double?"
            }, default(double?)},
            {new List<string>
            {
                "float",
                "Single"
            }, default(float)},
            {new List<string>
            {
                "float?",
                "Single?"
            }, default(float?)},
            {new List<string>
            {
                "decimal",
                "Decimal"
            }, default(decimal)},
            {new List<string>
            {
                "decimal?",
                "Decimal?"
            }, default(decimal?)},
            {new List<string>
            {
                "char",
                "Char"
            }, default(char)},
            {new List<string>
            {
                "char?",
                "Char?"
            }, default(char?)}
        };

        internal static string Map(string type)
        {
            return Mappings
                .First(k => k.Key.Any(t => t == type))
                .Value
                ?.ToString();
        }

        internal static string MapArray(ObjectModel type)
        {
            return $"[{Mappings
                .First(k => k.Key.Any(t => t == type.GenericType))
                .Value ?? "null"}]";
        }

        internal static string MapObject(ObjectModel type)
        {
            return "{}";
        }
    }
}
