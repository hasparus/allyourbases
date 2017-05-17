using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Sophie.Utils;

namespace Sophie
{
    public static partial class Logic
    {
        public static JObject Call(string functionName, JObject parameters) =>
            Functions[functionName]?.Invoke(parameters).ToJObject() ??
            Result.NotImplemented.ToJObject();

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
                return Result.Error.ToString();
            }

            if (json == null 
                || json.Type != JTokenType.Object
                || !json.First.HasValues
                || json.First.Type != JTokenType.Property
                || json.First.First.Type != JTokenType.Object)
            {
                return Result.Error.ToString();
            }

            var propertyName = ((JProperty) json.First).Name;
            var contents = (JObject) json.First.First;

            return Call(propertyName, contents).ToString(Formatting.None);
        }

        private static readonly ReferenceHash<string, Func<JObject, Result>> Functions =
            new ReferenceHash<string, Func<JObject, Result>>
            {
                {"open", Open},
                {"organizer", x => throw new NotImplementedException()},
                {"event", x => throw new NotImplementedException()},
            };

        private static NpgsqlConnection _connection;

        private static Result Open(JObject parameters)
        {
            string dbName = parameters["baza"].ToString();
            string login = parameters["login"].ToString();
            string password = parameters["password"].ToString();
            if (new[] {dbName, login, password}.Any(x => x == null))
                return Error("Method Open got null parameters.");

            _connection = new NpgsqlConnection(
                //$"Server=127.0.0.1;Port=5432;Database={dbName};User Id={login};Password={password};"
                $"Host=localhost;Username={login};Password=" +
                $"{MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(password))}" +
                $";Database={dbName}");

            if (_connection == null)
                return Error("Coudn't create NpgsqlConnection.");

            _connection.Open();

            if ((_connection.State & ConnectionState.Open) == 0)
                return Error("Coudn't open connection to database.");

            return Result.Ok;
        }

        private static Result Error(string message)
        {
            Console.Error.WriteLine(message);
            return Result.Error;
        }

    }
}
