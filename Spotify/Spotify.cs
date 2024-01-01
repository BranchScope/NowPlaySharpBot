using System.Text.Json;
using System.Web;
using System.Xml;
using NowPlaySharpBot.TelegramApi;
using Npgsql;
using RestSharp;

namespace NowPlaySharpBot.Spotify;

public class Spotify
{
    private const string AuthResource = "https://accounts.spotify.com";
    private const string ApiResource = "https://api.spotify.com/v1";
    private const string RedirectResource = "https://nps.heromapia4.keenetic.link";
    private static readonly string? ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
    private static readonly string? ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    private const string ApiScopes = "user-read-currently-playing user-read-recently-played";
    
    private static readonly RestClientOptions ApiOptions = new RestClientOptions(ApiResource) {
        ThrowOnAnyError = false,
        ThrowOnDeserializationError = false
    };
    private static readonly RestClient ApiClient = new RestClient(options: ApiOptions);
    
    private static readonly RestClientOptions AuthOptions = new RestClientOptions(AuthResource) {
        ThrowOnAnyError = false,
        ThrowOnDeserializationError = false
    };
    private static readonly RestClient AuthClient = new RestClient(options: AuthOptions);

    // https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public string GenAuthUrl(string state)
    {
        return $"{AuthResource}/authorize?response_type=code&client_id={ClientId}&scope={HttpUtility.UrlEncode(ApiScopes)}&redirect_uri={RedirectResource + "/login"}&state={state}";
    }

    // https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public async Task<AuthResponse> GetAccessToken(string code)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ClientId + ":" + ClientSecret);
        var base64Credentials = Convert.ToBase64String(plainTextBytes);
        var request = new RestRequest("api/token", Method.Post);
        request.AddParameter("code", code);
        request.AddParameter("redirect_uri", RedirectResource + "/login");
        request.AddParameter("grant_type", "authorization_code");
        request.AddHeader("Authorization", $"Basic {base64Credentials}");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        
        var response = await AuthClient.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<AuthResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }

    // https://developer.spotify.com/documentation/web-api/tutorials/refreshing-tokens
    public async Task<bool> RefreshToken(string refreshToken, long userId, NpgsqlConnection db)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ClientId + ":" + ClientSecret);
        var base64Credentials = Convert.ToBase64String(plainTextBytes);
        var request = new RestRequest("api/token", Method.Post);
        request.AddParameter("refresh_token", refreshToken);
        request.AddParameter("grant_type", "refresh_token");
        request.AddParameter("client_id", ClientId);
        request.AddHeader("Authorization", $"Basic {base64Credentials}");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        
        var response = await AuthClient.ExecutePostAsync(request);
        Console.WriteLine(response.Content);
        var deserializedResponse = JsonSerializer.Deserialize<AuthResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
        if (deserializedResponse.Error == "invalid_grant")
        {
            return false;
        }
        await Database.Database.UpdateAccessToken(db, deserializedResponse, userId);
        return true;
    }
    
    // https://developer.spotify.com/documentation/web-api/reference/get-the-users-currently-playing-track
    public async Task<CurrentlyPlayingResponse> GetCurrentlyPlaying(string code, long userId, NpgsqlConnection db)
    {
        var request = new RestRequest("me/player/currently-playing", Method.Get);
        request.AddHeader("Authorization", $"Bearer {code}");

        var response = await ApiClient.ExecuteGetAsync(request);
        var deserializedResponse = JsonSerializer.Deserialize<CurrentlyPlayingResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
        if (deserializedResponse.Error != null)
        {
            var refreshStatus = await RefreshToken(code, userId, db);
            if (refreshStatus == false)
            {
                return null;
            }
            return await GetCurrentlyPlaying(code, userId, db);
        }
        
        return deserializedResponse;
    }
    
    // https://developer.spotify.com/documentation/web-api/reference/get-recently-played
    public async Task<RecentlyPlayedResponse> GetRecentlyPlayed(string? code)
    {
        var request = new RestRequest("me/player/recently-played", Method.Get);
        request.AddHeader("Authorization", $"Bearer {code}");
        request.AddParameter("limit", 4);

        var response = await ApiClient.ExecuteGetAsync(request);
        return JsonSerializer.Deserialize<RecentlyPlayedResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    // https://developer.spotify.com/documentation/web-api/reference/get-track
    public async Task<TrackResponse> GetTrack(string id, string code)
    {
        {
            var request = new RestRequest($"tracks/{id}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {code}");

            var response = await ApiClient.ExecuteGetAsync(request);
            return JsonSerializer.Deserialize<TrackResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
        }
    }

    public async Task<string> Login(string code, string state, NpgsqlConnection db)
    {
        var auth = await GetAccessToken(code);
        await Database.Database.AddTokens(db, auth, state);
        return !string.IsNullOrEmpty(auth.Error) ? "Something went wrong, please retry." : "Well, go back to the bot now,";
    }
}