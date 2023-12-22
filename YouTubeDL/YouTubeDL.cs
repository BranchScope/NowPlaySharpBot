using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NowPlaySharpBot.YouTubeDL;

public sealed class YouTubeDL
{
    private const string Resource = "https://music.youtube.com";
    private const string YtDlpPath = "yt-dlp.exe";
    private const string RePattern = @"\[ExtractAudio\] Destination: (.+)";

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
    public static async Task<dynamic?> Download(string query)
    {
        var process = new ProcessStartInfo
        {
            FileName = YtDlpPath,
            Arguments = $"-f bestaudio -x --audio-format mp3 --audio-quality 320k --add-metadata --output \"%(title)s.%(ext)s\" \"{Resource}/search?q={query}\" --playlist-items 1",
            RedirectStandardOutput = true,
        };
        var proc = Process.Start(process);
        await proc.WaitForExitAsync();
        var logs = await proc.StandardOutput.ReadToEndAsync();
        var extractedAudio = ExtractedAudioReSearch(logs);
        return extractedAudio ?? null;
    }
}