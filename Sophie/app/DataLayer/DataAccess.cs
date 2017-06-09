using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Sophie.DataLayer
{
    public class DataAccess
    {
        private readonly string _connectionString;
        private NpgsqlConnection _mainConnection;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CallResult ExecuteSqlFromFile(
            string filename,
            NpgsqlConnection connection = null)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var setupFile = new StreamReader(fs))
                return ExecuteSqlFromString(setupFile.ReadToEnd());
        }

        public CallResult ExecuteSqlFromString(
            string s,
            NpgsqlConnection connection = null)
        {
            try
            {
                //Debug.Log(" executing ~~> " + s);

                connection = connection ?? ProvideConnection();
                connection.TryOpen();

                using (var cmd = new NpgsqlCommand(s, connection))
                {
                    var reader = cmd.ExecuteReader();
                    JArray data = new JArray();
                    while (reader.Read())
                    {
                        var x = reader.GetString(0);
                        CallResult? breakingResult = null;

                        if (reader.FieldCount == 1)
                            breakingResult = x == CallResult.DbError 
                                ? CallResult.Error("Conference API authorization failed.") 
                                : CallResult.Ok;

                        if (breakingResult != null)
                        {
                            reader.Dispose();
                            return breakingResult.Value;
                        }

                        // db returned data:
                        data.Add(
                            new JObject(
                                Enumerable.Range(0, reader.FieldCount).Select(
                                    index =>
                                        new JProperty(reader.GetName(index), 
                                        typeof(DBNull) == reader.GetValue(index).GetType()
                                            ? "null" : reader.GetValue(index)
                                        )
                                    )
                                )
                            );
                    }
                    reader.Dispose();
                    connection.Close();
                    //connection.Dispose();
                    //Debug.Log("Output from db: " + data);
                    return new CallResult(CallResult.Status.Ok, data);
                }
            }
            catch (PostgresException e)
            {
                connection?.Close();
                //connection?.Dispose();
                return CallResult.Error("Postgres exception catched in ExecuteSqlFromString. " + e.Message);
            }
        }

        public NpgsqlConnection ProvideConnection()
        {
            try
            {
                if (_mainConnection == null)
                    _mainConnection = new NpgsqlConnection(_connectionString);
                return _mainConnection;
            }
            catch (PostgresException)
            {
                return null;
            }
        }
    }
}