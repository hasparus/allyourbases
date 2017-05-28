using System;
using Newtonsoft.Json.Linq;

namespace Sophie.Utils
{
    public static class JTokenExtensions
    {
        public static string ToSqlString(this JToken jToken)
        {
            if (jToken.Type == JTokenType.String)
                return $"'{jToken}'";
            if (jToken.Type == JTokenType.Null)
                return "NULL";
            return $"{jToken}";
        }
    }
}
