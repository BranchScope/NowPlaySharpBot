﻿using NowPlaySharpBot.Spotify;
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
        cmd.Parameters.AddWithValue("last_name", user.LastName ?? "");
        cmd.Parameters.AddWithValue("username", user.Username ?? "");
        cmd.Parameters.AddWithValue("lang", user.LanguageCode ?? "");
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
    }

    public static async Task<int> AddTokens(NpgsqlConnection db, AuthResponse auth, string state)
    {
        var user_id = BitConverter.ToInt64(Convert.FromBase64String(state));
        var query = "INSERT INTO tokens(user_id, access_token, refresh_token) VALUES(@user_id, @access_token, @refresh_token)";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("user_id", user_id);
        cmd.Parameters.AddWithValue("access_token", auth.AccessToken);
        cmd.Parameters.AddWithValue("refresh_token", auth.RefreshToken);
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
    }

    public static async Task<List<object>> GetTokens(NpgsqlConnection db, long user_id)
    {
        var cmd = new NpgsqlCommand($"SELECT access_token,refresh_token FROM tokens WHERE user_id = @user_id", db);
        cmd.Parameters.AddWithValue("user_id", user_id);
        await cmd.PrepareAsync();
        var dr = await cmd.ExecuteReaderAsync();
        
        if (dr.Read() == false)
        {
            await dr.CloseAsync();
            return [];
        }

        var tokens = new List<object> { dr[0], dr[1] };
        await dr.CloseAsync();
        return tokens;
    }
    
    public static async Task<int> UpdateAccessToken(NpgsqlConnection db, AuthResponse auth, long user_id)
    {
        var query = "UPDATE tokens SET access_token = @access_token WHERE user_id = @user_id";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("access_token", auth.AccessToken);
        cmd.Parameters.AddWithValue("user_id", user_id);
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
    }

    public static async Task<int> AddMusic(NpgsqlConnection db, string songId, string title, string artist, string? album, string? thumbnail, string fileId)
    {
        var query = "INSERT INTO music(song_id, title, artist, album, thumbnail, file_id) VALUES(@song_id, @title, @artist, @album, @thumbnail, @file_id)";
        var cmd = new NpgsqlCommand(query, db);
        cmd.Parameters.AddWithValue("song_id", songId);
        cmd.Parameters.AddWithValue("title", title);
        cmd.Parameters.AddWithValue("artist", artist);
        cmd.Parameters.AddWithValue("album", album);
        cmd.Parameters.AddWithValue("thumbnail", thumbnail);
        cmd.Parameters.AddWithValue("file_id", fileId);
        await cmd.PrepareAsync();
        var r = await cmd.ExecuteNonQueryAsync();
        return r;
    }
    
    public static async Task<string?> GetMusic(NpgsqlConnection db, string songId)
    {
        var cmd = new NpgsqlCommand($"SELECT file_id FROM music WHERE song_id = @song_id", db);
        cmd.Parameters.AddWithValue("song_id", songId);
        await cmd.PrepareAsync();
        var dr = await cmd.ExecuteReaderAsync();
        
        if (dr.Read() == false)
        {
            await dr.CloseAsync();
            return null;
        }

        var fileId = dr[0].ToString();
        await dr.CloseAsync();
        return fileId;
    }
    
}