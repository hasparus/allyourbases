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

        public static string ExecuteInputLine(string line)
        {
            JToken json;

            try
            {
                json = JToken.Parse(line);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("My dear, this is not proper json.");
                return Logic.Result.Error.ToString();
            }

            if (!ValidFunctionCallJson(json))
                return Logic.Result.Error.ToString();

            var propertyName = ((JProperty) json.First).Name;
            var contents = (JObject) json.First.First;

            return Logic.Call(propertyName, contents).ToString(Formatting.None);
        }

        private static bool ValidFunctionCallJson(JToken json)
        {
            return json == null
                            || json.Type != JTokenType.Object
                            || !json.First.HasValues
                            || json.First.Type != JTokenType.Property
                            || json.First.First.Type != JTokenType.Object;
        }
    }
}
