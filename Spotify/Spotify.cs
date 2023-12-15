using System.Net;
using System.Text.Json;
using System.Web;
using RestSharp;

namespace NowPlaySharpBot.Spotify;

public class Spotify
{
    private const string AuthResource = "https://accounts.spotify.com";
    private const string ApiResource = "https://api.spotify.com/v1";
    private const string RedirectResource = "https://nps.branchscope.eu.org";
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

    public static string GenAuthUrl(string state)
    {
        return $"{AuthResource}/authorize?response_type=code&client_id={ClientId}&scope={HttpUtility.UrlEncode(ApiScopes)}&redirect_uri={RedirectResource + "/login"}&state={state}";
    }


    public static async Task<dynamic?> GetAccessToken(string code)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ClientId + ":" + ClientSecret);
        var base64Credentials = System.Convert.ToBase64String(plainTextBytes);
        var request = new RestRequest("api/token", Method.Post);
        request.AddParameter("code", code);
        request.AddParameter("redirect_uri", RedirectResource + "/login");
        request.AddParameter("grant_type", "authorization_code");
        request.AddHeader("Authorization", $"Basic {base64Credentials}");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        
        var response = await AuthClient.ExecutePostAsync(request);
        return JsonSerializer.Serialize(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }

    public static async Task<dynamic> RefreshToken(string refreshToken)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ClientId + ":" + ClientSecret);
        var base64Credentials = System.Convert.ToBase64String(plainTextBytes);
        var request = new RestRequest("api/token", Method.Post);
        request.AddParameter("refresh_token", refreshToken);
        request.AddParameter("grant_type", "refresh_token");
        request.AddParameter("client_id", ClientId);
        request.AddHeader("Authorization", $"Basic {base64Credentials}");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        
        var response = await AuthClient.ExecutePostAsync(request);
        return JsonSerializer.Serialize(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    public static async Task<dynamic?> GetCurrentlyPlaying(string code)
    {
        var request = new RestRequest("me/player/currently-playing", Method.Get);
        request.AddHeader("Authorization", $"Bearer {code}");

        var response = await ApiClient.ExecuteGetAsync(request);
        return JsonSerializer.Serialize(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    public static async Task<dynamic?> GetRecentlyPlayed(string code)
    {
        var request = new RestRequest("me/player/recently-played", Method.Get);
        request.AddHeader("Authorization", $"Bearer {code}");

        var response = await ApiClient.ExecuteGetAsync(request);
        return JsonSerializer.Serialize(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
}