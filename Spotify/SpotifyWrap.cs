namespace NowPlaySharpBot.Spotify;

public class SpotifyWrap
{
    private static Database.Database _db;
    private static Spotify _api = new Spotify();

    public SpotifyWrap(Database.Database db)
    {
        _db = db;
    }

    public string GenAuthUrl(string state) => _api.GenAuthUrl(state);
    public static async Task<AuthResponse> GetAccessToken(string code) => await _api.GetAccessToken(code);
    public static async Task<bool> RefreshToken(string refreshToken, long userId) => await _api.RefreshToken(refreshToken, userId, _db.db);
    public static async Task<CurrentlyPlayingResponse> GetCurrentlyPlaying(string code, long userId) => await _api.GetCurrentlyPlaying(code, userId, _db.db);
    public static async Task<RecentlyPlayedResponse> GetRecentlyPlayed(string code) => await _api.GetRecentlyPlayed(code);
    public static async Task<TrackResponse> GetTrack(string Id, string code) => await _api.GetTrack(Id, code);
    public static async Task<string> Login(string code, string state) => await _api.Login(code, state, _db.db);
}