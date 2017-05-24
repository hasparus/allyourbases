using System.IO;
using Npgsql;

namespace Sophie.DataLayer
{
    public class DataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CallResult ExecuteSqlFromFile(
            string filename,
            NpgsqlConnection connection = null)
        {
            connection = connection ?? new NpgsqlConnection(_connectionString);

            using (var fs = new FileStream(filename, FileMode.Open))
            using (var setupFile = new StreamReader(fs))
                return ExecuteSqlFromString(setupFile.ReadToEnd(), connection);
        }

        public CallResult ExecuteSqlFromString(
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
                        if (x == "0") return CallResult.Error("Conference API authorization failed."); ;
                    }
                return CallResult.Ok;
            }
            catch (PostgresException e)
            {
                return CallResult.Error("Postgres exception catched in ExecuteSqlFromString. " + e.Message);
            }
        }

        public NpgsqlConnection NewConnection()
        {
            try
            {
                return new NpgsqlConnection(_connectionString);
            }
            catch (PostgresException)
            {
                return null;
            }
        }
    }
}