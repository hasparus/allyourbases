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
                $"Host=localhost;Username={login};Password={password};Database={dbName}");

            if (_connection == null)
                return Error("Coudn't create NpgsqlConnection.");

            _connection.Open();

            if ((_connection.State & ConnectionState.Open) == 0)
                return Error("Coudn't open connection to database.");

            return Result.Ok;
        }
    }
}
