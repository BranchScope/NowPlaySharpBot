using System.Text.Json.Serialization;

namespace NowPlaySharpBot.Spotify;

public record AuthResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    [JsonPropertyName("scope")] public string? Scope { get; init; }
    
    // error handling
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("error_description")] public string? ErrorDescription { get; init; }
}

public record CurrentlyPlayingResponse
{
    [JsonPropertyName("item")] public Item? Item { get; init; }
    
    // error handling
    [JsonPropertyName("error")] public Error? Error { get; init; }
}

public record RecentlyPlayedResponse
{
    [JsonPropertyName("items")] public List<Track>? Items { get; init; }
    [JsonPropertyName("error")] public Error? Error { get; init; }
}

public record Track
{
    [JsonPropertyName("track")] public Item? Item { get; init; }
}

public record Item
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("album")] public Album? Album { get; init; }
    [JsonPropertyName("artists")] public List<Artist>? Artists { get; init; }
    [JsonPropertyName("preview_url")] public string? PreviewUrl { get; init; }
    [JsonPropertyName("duration_ms")] public int? DurationMs { get; init; }
}

public record Album
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("images")] public List<Images>? Images { get; init; }
}

public record Images
{
    [JsonPropertyName("url")] public string? Url { get; init; }
}

public record Artist
{
    [JsonPropertyName("name")] public string? Name { get; init; }
}

public record Error
{
    [JsonPropertyName("status")] public int? Status { get; init; }
    [JsonPropertyName("message")] public string? Message { get; init; }
}
