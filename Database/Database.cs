using System.Text.Json;
using System.Text.Json.Nodes;
using Npgsql;
using NpgsqlTypes;

namespace NowPlaySharpBot.Database;

public class Database
{
    private NpgsqlConnection db { get; set; }
    private static readonly string? PgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
    private static readonly string? PgUsername = Environment.GetEnvironmentVariable("POSTGRES_USER");
    private static readonly string? PgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    private static readonly string? PgDatabaseName = Environment.GetEnvironmentVariable("POSTGRES_DBNAME");

    private static readonly string PgConnectionString = $"Host={PgHost};Username={PgUsername};Password={PgPassword};Database={PgDatabaseName}";

    public static async Task<NpgsqlConnection> Connect()
    {
        var con = new NpgsqlConnection(
            connectionString: PgConnectionString);
        con.Open();
        return con;
    }

    public static async Task<object?> CheckUser(NpgsqlConnection db, long user_id)
    {
        var query = "SELECT COUNT(*) FROM users WHERE user_id = @user_id";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("user_id", user_id);
        await cmd.PrepareAsync();
        var count = await cmd.ExecuteScalarAsync();
        return count;
    }

    public async Task<dynamic?> Select(NpgsqlConnection db, long user_id)
    {
        var cmd = new NpgsqlCommand($"SELECT * FROM users", db);
        //cmd.Parameters.AddWithValue("user_id", user_id);
        await cmd.PrepareAsync();
        var dr = await cmd.ExecuteReaderAsync();
        while (dr.Read())
            Console.Write("{0}\t{1} \n", dr[0], dr[1]);
        return false;
    }
    
}