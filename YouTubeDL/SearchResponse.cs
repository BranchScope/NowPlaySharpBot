using System.Net.Mime;
using System.Text.Json.Serialization;

namespace NowPlaySharpBot.YouTubeDL;

// this is a selective parsing of the content, most of the shit is not recorded because useless
// the specific section is retrieved arbitrarily deserializing the big json as a unknown object
public record SearchResponse
{
    [JsonPropertyName("contents")] public Contents? _contents { get; init; }

    public record Contents
    {
        [JsonPropertyName("tabbedSearchResultsRenderer")] public TabbedSearchResultsRenderer _tabbedSearchResultsRenderer { get; init; }

        public record TabbedSearchResultsRenderer
        {
            [JsonPropertyName("tabs")] public List<Tabs?> _tabs { get; init; }

            public record Tabs
            {
                [JsonPropertyName("tabRenderer")] public TabRenderer _tabRenderer { get; init; }

                public record TabRenderer
                {
                    [JsonPropertyName("content")] public Content _content { get; init; }

                    public record Content
                    {
                        [JsonPropertyName("sectionListRenderer")] public SectionListRenderer _sectionListRenderer { get; init; }

                        public record SectionListRenderer
                        {
                            [JsonPropertyName("contents")] public List<Contents?> _contents { get; init; }

                            public record Contents
                            {
                                [JsonPropertyName("musicShelfRenderer")] public MusicShelfRenderer _musicShelfRenderer { get; init; }

                                public record MusicShelfRenderer
                                {
                                    [JsonPropertyName("contents")] public List<TrackContents?> _contents { get; init; }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

public record TrackContents
{
    [JsonPropertyName("musicResponsiveListItemRenderer")] public MusicResponsiveListItemRenderer? _musicResponsiveListItemRenderer { get; init; }
}

public record MusicResponsiveListItemRenderer
{
    [JsonPropertyName("thumbnail")] public Thumbnail? _thumbnail { get; init; }
    [JsonPropertyName("flexColumns")] public List<FlexColumns?> _flexColumns { get; init; }
}

public record Thumbnail
{
    [JsonPropertyName("musicThumbnailRenderer")] public MusicThumbnailRenderer? _musicThumbnailRenderer { get; init; }

    public record MusicThumbnailRenderer
    {
        [JsonPropertyName("thumbnail")] public thumbnail? _thumbnail { get; init; }

        public record thumbnail
        {
            [JsonPropertyName("thumbnails")] public List<Thumbnails?> _thumbnails { get; init; }

            public record Thumbnails
            {
                [JsonPropertyName("url")] public string? _url { get; init; }
            }
        }
    }
}

public record FlexColumns
{
    [JsonPropertyName("musicResponsiveListItemFlexColumnRenderer")] public MusicResponsiveListItemFlexColumnRenderer _musicResponsiveListItemFlexColumnRenderer { get; init; }

    public record MusicResponsiveListItemFlexColumnRenderer
    {
        [JsonPropertyName("text")] public Text _text { get; init; }

        public record Text
        {
            [JsonPropertyName("runs")] public List<Runs?> _runs { get; init; }

            public record Runs
            {
                [JsonPropertyName("text")] public string? _text { get; init; }
                [JsonPropertyName("navigationEndpoint")] public NavigationEndpoint? _navigationEndpoint { get; init; }

                public record NavigationEndpoint
                {
                    [JsonPropertyName("watchEndpoint")] public WatchEndpoint? _watchEndpoint { get; init; }

                    public record WatchEndpoint
                    {
                        [JsonPropertyName("videoId")] public string? _videoId { get; init; }
                    }
                }
            }
        }
    }
}