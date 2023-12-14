using System.Text.Json;
using RestSharp;

namespace NowPlaySharpBot.Deezer;

public static class Deezer
{
    
    private const string Resource = "https://api.deezer.com";

    private static readonly RestClientOptions Options = new RestClientOptions(Resource) {
        ThrowOnAnyError = false,
        ThrowOnDeserializationError = false
    };
    private static readonly RestClient Client = new RestClient(options: Options);
    
    public static async Task<TrackResponse> Search(string query)
    {
        var request = new RestRequest("search/track", Method.Post);
        request.AddParameter("q", query);
        var response = await Client.ExecuteGetAsync(request);
        return JsonSerializer.Deserialize<TrackResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");;
    }
}