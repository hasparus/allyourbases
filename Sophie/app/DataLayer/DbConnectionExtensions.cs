using System.Data;

namespace Sophie.DataLayer
{
    internal static class DbConnectionExtensions
    {
        public static bool IsOpen(this IDbConnection con)
            => (con.State & ConnectionState.Open) != 0;

        public static void TryOpen(this IDbConnection con)
        {
            if (!con.IsOpen())
                con.Open();
        }
    }
}