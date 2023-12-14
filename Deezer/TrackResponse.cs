using System.Text.Json.Serialization;

namespace NowPlaySharpBot;

public record TrackResponse
{
    [JsonPropertyName("data")] public List<Data>? Data { get; init; }
}

public record Data
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("title")] public string Title { get; init; }
    [JsonPropertyName("artist")] public Artist Artist { get; init; }
    [JsonPropertyName("album")] public Album Album { get; init; }
    [JsonPropertyName("duration")] public int Duration { get; init; }
    [JsonPropertyName("preview")] public string Preview { get; init; }
}

public record Artist
{
    [JsonPropertyName("name")] public string Name { get; init; }
}

public record Album
{
    [JsonPropertyName("cover_big")] public string ImgUrl { get; init; }
}
