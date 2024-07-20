using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using RestSharp;

namespace NowPlaySharpBot.YouTubeDL;

public sealed class YouTubeDL
{
    private const string Resource = "https://music.youtube.com";
    private const string YtDlpPath = "yt-dlp";
    private const string RePattern = @"\[ExtractAudio\] Destination: (.+)";
    private static readonly RestClientOptions Options = new RestClientOptions("https://music.youtube.com/youtubei/v1")
    {
        ThrowOnAnyError = false,
        ThrowOnDeserializationError = false,
        MaxTimeout = -1,
    };
    private static readonly RestClient Client = new RestClient(options: Options);

    // little parsing util
    private static string? ExtractedAudioReSearch(string logs)
    {
        var match = Regex.Match(logs, RePattern, RegexOptions.IgnoreCase);
        return (match.Success ? match.Groups[1] : null)?.ToString();
    }
    
    /// <summary>
    /// Yep, it's such a rough method, but there are no official APIs for YouTube Music search
    /// There are no checks there either, but it is well known that YouTube Music has more music than other platforms
    /// (They are all excuses to the fact that I don't want to create a downloader for Deezer :P)
    /// </summary>
    public static async Task<string?> Download(string query, string songId)
    {
        var process = new ProcessStartInfo
        {
            FileName = YtDlpPath,
            Arguments = $"-f bestaudio -x --audio-format mp3 --audio-quality 320k --add-metadata --output \"{songId}.%(ext)s\" \"{Resource}/search?q={query}#songs\" --playlist-items 1",
            RedirectStandardOutput = true,
            WorkingDirectory = "/app/workdir"
        };
        var proc = Process.Start(process);
        await proc.WaitForExitAsync();
        var logs = await proc.StandardOutput.ReadToEndAsync();
        var extractedAudio = ExtractedAudioReSearch(logs);
        return extractedAudio ?? null;
    }
    
    // search func using YT Music direct url
    public static async Task<List<Dictionary<string, object>>?> Search(string query)
    {
        var request = new RestRequest("search?prettyPrint=false", Method.Post);
        request.AddHeader("content-type", "application/json");
        var body = $@"{{
            ""context"": {{
                ""client"": {{
                    ""hl"": ""en"",
                    ""clientName"": ""WEB_REMIX"",
                    ""clientVersion"": ""1.20240709.02.00""
                }}
            }},
            ""query"": ""{query}"",
            ""params"": ""EgWKAQIIAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D""
        }}";
        request.AddStringBody(body, DataFormat.Json);
        var response = await Client.ExecutePostAsync(request);
        try
        {
            var json = JsonSerializer.Deserialize<SearchResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("Failed to deserialize response.");

            var songs = json._contents?._tabbedSearchResultsRenderer?._tabs?
                .SelectMany(tab => tab?._tabRenderer?._content?._sectionListRenderer?._contents ?? new List<SearchResponse.Contents.TabbedSearchResultsRenderer.Tabs.TabRenderer.Content.SectionListRenderer.Contents>())
                .Where(section => section?._musicShelfRenderer != null)
                .SelectMany(section => section._musicShelfRenderer._contents)
                .Select(content => new
                {
                    Title = content?._musicResponsiveListItemRenderer?._flexColumns
                        ?.ElementAtOrDefault(0)?._musicResponsiveListItemFlexColumnRenderer?._text?._runs
                        ?.ElementAtOrDefault(0)?._text,
                    Artists = content?._musicResponsiveListItemRenderer?._flexColumns
                        ?.ElementAtOrDefault(1)?._musicResponsiveListItemFlexColumnRenderer?._text?._runs
                        ?.Select(run => run?._text)
                        .FirstOrDefault()
                        .Split(new[] { " • " }, StringSplitOptions.None)
                        .FirstOrDefault()
                        .Replace("&", ",")
                        .Replace(",,", ",")
                        .Trim() // Remove leading/trailing spaces
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Trim())
                        .ToList(),
                    ThumbnailUrl = content?._musicResponsiveListItemRenderer?._thumbnail?._musicThumbnailRenderer?._thumbnail?._thumbnails?.LastOrDefault()?._url,
                    VideoId = content?._musicResponsiveListItemRenderer?._flexColumns
                        ?.Select(fc => fc?._musicResponsiveListItemFlexColumnRenderer?._text?._runs
                            .Where(run => run?._navigationEndpoint?._watchEndpoint?._videoId != null)
                            .Select(run => run?._navigationEndpoint?._watchEndpoint?._videoId)
                            .FirstOrDefault())
                        .FirstOrDefault()
                })
                .Where(song => song.Title != null && song.Artists != null && song.ThumbnailUrl != null && song.VideoId != null);
            
            return songs.Select(song => new Dictionary<string, object>
            {
                { "title", song.Title },
                { "artists", song.Artists },
                { "thumbnailUrl", song.ThumbnailUrl },
                { "videoId", song.VideoId }
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            return null;
        }
    }
}