using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sophie.Utils;

namespace Sophie.IO
{
    public class IoController
    {
        public IoController()
        {
            // [ Note ]
            // Bad news: .NET Core lacks OpenStandardInput(int) method
            // for extending ReadLine buffer size.
            // Good news: In .NET Core console app running on Ubuntu
            // there is no limit on ReadLine buffer size.
        }

        private static string SkipCommentAndEmptyLine(string line, bool silence = true)
            => line.Trim().Length == 0 || line[0] == '#'
               ? silence ? "" : line
               : null;

        public static string ExecuteInputLine(string line)
        {
            var comment = SkipCommentAndEmptyLine(line, false);
            if (comment != null)
            {
                if (comment != "") Debug.Log(comment);
                return "";
            }

            JToken json;

            try
            {
                json = JToken.Parse(line);
            }
            catch (JsonReaderException e)
            {
                Debug.Log(e.Message);
                return CallResult.Error("Excuse me, this is not proper json.").ToString();
            }

            if (!ValidFunctionCallJson(json))
                return CallResult.Error("This json is not valid function call.").ToString();

            var propertyName = ((JProperty) json.First).Name;
            var contents = (JObject) json.First.First;

            return Logic.Call(propertyName, contents).ToString(Formatting.None);
        }

        private static bool ValidFunctionCallJson(JToken json)
        {
            return json != null 
                && json.Type == JTokenType.Object 
                && json.First.HasValues 
                && json.First.Type == JTokenType.Property 
                && json.First.First.Type == JTokenType.Object;
        }
    }
}
