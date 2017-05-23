using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Sophie.DataLayer;
using Sophie.Utils;

namespace Sophie
{
    public static class Logic
    {
        private static DataAccess _dataAccess;

        public static JObject Call(string functionName, JObject parameters) =>
            Functions[functionName]?.Invoke(parameters).ToJObject() ??
            CallResult.NotImplemented.ToJObject();

        private static Func<JObject, CallResult> SqlProcWrapper(string procName, params string[] parameters)
        {
            return args =>
            {
                if (!ValidCallParameters(args, parameters))
                    return CallResult.Error(
                        $"Invalid parameters for {procName} sql function.");
                return _dataAccess.ExecuteSqlFromString(
                    $"select {procName}("
                    + string.Join(", ", parameters.Select(key => $"'{args[key]}'"))
                    + ")");
            };
        }

        private static readonly ReferenceHash<string, Func<JObject, CallResult>> Functions =
            new ReferenceHash<string, Func<JObject, CallResult>>
            {
                {"open", Open},
                {"drop_the_base", args =>
                    {
                        if (!ValidCallParameters(args, new[] {"secret"})
                            || args["secret"].ToString() != "42")
                            return CallResult.Error("Haha. No dropping if you don't know secret.");
                        return _dataAccess.ExecuteSqlFromFile("db/drop.sql");
                    }
                },
                {"organizer",
                    SqlProcWrapper("add_organiser", "secret", "newlogin", "newpassword")
                },
                {"event",
                    SqlProcWrapper("add_event", "login", "password", "eventname", "start_timestamp", "end_timestamp")},
            };

        private static CallResult Open(JObject parameters)
        {
            if (!ValidCallParameters(parameters, new[] { "baza", "login", "password" }))
                return CallResult.Error("Method Open got wrong parameters.");

            string dbName = parameters["baza"].ToString();
            string login = parameters["login"].ToString();
            string password = parameters["password"].ToString();

            _dataAccess = new DataAccess($"Host=localhost;Username={login};Password={password};Database={dbName}");
            try
            {
                using (var connection = _dataAccess.NewConnection())
                {
                    connection.Open();

                    return connection.IsOpen()
                        ? _dataAccess.ExecuteSqlFromFile("db/open.sql", connection)
                        : CallResult.Error("Coudn't open connection to database.");
                }
            }
            catch (PostgresException e)
            {
                return CallResult.Error(e.Message);
            }
            catch (SocketException)
            {
                Debug.Log("Please enable socket access to PostgreSQL server.");
                throw;
            }
        }

        private static bool ValidCallParameters(JObject given, IEnumerable<string> requested)
            => requested.All(p => given[p] != null);
    }
}