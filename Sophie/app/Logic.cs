using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

        private static Func<JObject, Result> newRedirectingFunc(string funcName, params string[] parameters)
        {
            return x =>
            {
                if (!ValidCallParameters(x, parameters))
                    return Error($"Invalid parameters for {funcName} method.");
                return ExecuteSqlFromString($"select ${funcName}("
                                            + string.Join(", ", parameters.Select(key => $"'{x[key]}'"))
                                            + ")");
            };
        }

        private static readonly ReferenceHash<string, Func<JObject, Result>> Functions =
            new ReferenceHash<string, Func<JObject, Result>>
            {
                {"open", Open},
                {"organizer", x =>
                    {
                        if (!ValidCallParameters(x, new[] {"secret", "newlogin", "newpassword"}))
                            return Error("Invalid parameters for Organizer method.");
                        return ExecuteSqlFromString(
                            $"select add_organiser('{x["secret"]}', '{x["newlogin"]}', '{x["newpassword"]}');");
                    }
                },
                {"event", x => throw new NotImplementedException()},
            };

        private static string _connectionString;

        private static bool ValidCallParameters(JObject given, IEnumerable<string> requested)
            => requested.All(p => given[p] != null);

        private static Result Open(JObject parameters)
        {
            if (!ValidCallParameters(parameters, new[] { "baza", "login", "password" }))
                return Error("Method Open got wrong parameters.");

            string dbName = parameters["baza"].ToString();
            string login = parameters["login"].ToString();
            string password = parameters["password"].ToString();

            _connectionString = $"Host=localhost;Username={login};Password={password};Database={dbName}";
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    if (!connection.IsOpen())
                        return Error("Coudn't open connection to database.");

                    return ExecuteSqlFromFile("db/open.sql", connection);
                }
            }
            catch (PostgresException e)
            {
                Debug.Log(e.Message);
                return Result.Error;
            }
        }

        public static Result ExecuteSqlFromFile(
            string filename,
            NpgsqlConnection connection = null)
        {
            connection = connection ?? new NpgsqlConnection(_connectionString);

            using (var fs = new FileStream(filename, FileMode.Open))
            using (var setupFile = new StreamReader(fs))
                return ExecuteSqlFromString(setupFile.ReadToEnd(), connection);
        }

        private static Result ExecuteSqlFromString(
            string s,
            NpgsqlConnection connection = null)
        {
            try
            {
                connection = connection ?? new NpgsqlConnection(_connectionString);
                connection.TryOpen();
                using (var cmd = new NpgsqlCommand(s, connection))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var x = reader.GetString(0);
                        Debug.Log("Output from db: " + x);
                        if (x == "0") return Result.Error; ;
                    }
                return Result.Ok;
            }
            catch (Npgsql.PostgresException e)
            {
                return Error("Postgres exception catched in ExecuteSqlFromString. " + e.Message);
            }
        }

        private static bool IsOpen(this IDbConnection con)
            => (con.State & ConnectionState.Open) != 0;

        private static void TryOpen(this IDbConnection con)
        {
            if (!con.IsOpen())
                con.Open();
        }

    }
}