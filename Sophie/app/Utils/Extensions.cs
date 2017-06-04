using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Sophie.Utils
{
    public static class JTokenExtensions
    {
        public static string ToSqlString(this JToken jToken)
        {
            if (jToken.Type == JTokenType.Null)
                return "NULL";

            var sanitized = jToken.ToString().Sanitize();

            if (jToken.Type == JTokenType.String)
                return $"'{sanitized}'";
            return $"{sanitized}";
        }
    }

    public static class StringExtensions
    {
        public static string Sanitize(this string s)
        {
            if (!Regex.IsMatch(s, @"^[żźćńółęąśŻŹĆĄŚĘŁÓŃa-zA-Z0-9\-+&_.:\s]+$"))
            {
                Console.WriteLine(s + " nie przeszło!");
                return " --";
            }
            return s;
        }
    }
}
