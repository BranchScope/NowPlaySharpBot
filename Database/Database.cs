using NowPlaySharpBot.Spotify;
using Npgsql;

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

    public static async Task<long> CheckUser(NpgsqlConnection db, long user_id)
    {
        var query = "SELECT COUNT(*) FROM users WHERE user_id = @user_id";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("user_id", user_id);
        await cmd.PrepareAsync();
        var count = (long)await cmd.ExecuteScalarAsync();
        return count;
    }

    public static async Task<int> AddUser(NpgsqlConnection db, User user)
    {
        var query = "INSERT INTO users(user_id, first_name, last_name, username, lang) VALUES(@user_id, @first_name, @last_name, @username, @lang)";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("user_id", user.Id);
        cmd.Parameters.AddWithValue("first_name", user.FirstName);
        cmd.Parameters.AddWithValue("last_name", user.LastName ?? null);
        cmd.Parameters.AddWithValue("username", user.Username ?? null);
        cmd.Parameters.AddWithValue("lang", user.LanguageCode ?? null);
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
    }

    public static async Task<int> AddTokens(NpgsqlConnection db, AuthResponse auth, string state)
    {
        var user_id = BitConverter.ToString(System.Convert.FromBase64String(state));
        var query = "INSERT INTO tokens(user_id, access_token, refresh_token) VALUES(@user_id, @access_token, @refresh_token)";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("user_id", user_id);
        cmd.Parameters.AddWithValue("access_token", auth.AccessToken);
        cmd.Parameters.AddWithValue("refresh_token", auth.RefreshToken);
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
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