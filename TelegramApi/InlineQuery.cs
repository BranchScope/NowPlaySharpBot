using System.Text.Json.Serialization;

namespace NowPlaySharpBot;

// https://core.telegram.org/bots/api#available-types

public class InlineQueryResultArticle
{
    [JsonPropertyName("type")]
    public string Type { get; init; }
    
    [JsonPropertyName("id")] 
    public string Id { get; init; }
    
    [JsonPropertyName("title")] 
    public string Title { get; init; }
    
    [JsonPropertyName("input_message_content")]
    public InputMessageContent InputMessageContent { get; init; }
    
    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InlineKeyboard? ReplyMarkup { get; init; }
    
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }
    
    [JsonPropertyName("hide_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HideUrl { get; init; }
    
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
    
    [JsonPropertyName("thumbnail_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailUrl { get; init; }
    
    [JsonPropertyName("thumbnail_width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ThumbnailWidth { get; init; }
    
    [JsonPropertyName("thumbnail_height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ThumbnailHeight { get; init; }

    public InlineQueryResultArticle(string type, string id, string title, InputMessageContent inputMessageContent, InlineKeyboard? replyMarkup = null, string? url = null, bool? hideUrl = false, string? description = null, string? thumbnailUrl = null, int? thumbnailWidth = null, int? thumbnailHeight = null)
    {
        Type = type;
        Id = id;
        Title = title;
        InputMessageContent = inputMessageContent;
        ReplyMarkup = replyMarkup;
        Url = url;
        HideUrl = hideUrl;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        ThumbnailWidth = thumbnailWidth;
        ThumbnailHeight = thumbnailHeight;
    }
}

public class InlineQueryResultCachedAudio
{
    [JsonPropertyName("type")]
    public string Type { get; init; }
    
    [JsonPropertyName("id")] 
    public string Id { get; init; }
    
    [JsonPropertyName("audio_file_id")]
    public string AudioFileId { get; init; }
    
    [JsonPropertyName("caption")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Caption { get; init; }
    
    [JsonPropertyName("parse_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; init; }
    
    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InlineKeyboard? ReplyMarkup { get; init; }
    
    [JsonPropertyName("input_message_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InputMessageContent? InputMessageContent { get; init; }

    public InlineQueryResultCachedAudio(string type, string id, string audioFileId, string? caption = null, string? parseMode = "HTML", InlineKeyboard? replyMarkup = null, InputMessageContent? inputMessageContent = null)
    {
        Type = type;
        Id = id;
        AudioFileId = audioFileId;
        Caption = caption;
        ParseMode = parseMode;
        ReplyMarkup = replyMarkup;
        InputMessageContent = inputMessageContent;
    }
    
}

public class InlineQueryResultAudio
{
    [JsonPropertyName("type")]
    public string Type { get; init; }
    
    [JsonPropertyName("id")] 
    public string Id { get; init; }
    
    [JsonPropertyName("audio_url")]
    public string AudioUrl { get; init; }
    
    [JsonPropertyName("title")]
    public string Title { get; init; }
    
    [JsonPropertyName("caption")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Caption { get; init; }
    
    [JsonPropertyName("parse_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; init; }
    
    [JsonPropertyName("performer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Performer { get; init; }
    
    [JsonPropertyName("audio_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AudioDuration { get; init; }
    
    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InlineKeyboard? ReplyMarkup { get; init; }
    
    [JsonPropertyName("input_message_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InputMessageContent? InputMessageContent { get; init; }
    
    public InlineQueryResultAudio(string type, string id, string audioUrl, string title, string? performer = null, int? audioDuration = null, string? caption = null, string? parseMode = "HTML", InlineKeyboard? replyMarkup = null, InputMessageContent? inputMessageContent = null)
    {
        Type = type;
        Id = id;
        AudioUrl = audioUrl;
        Title = title;
        Caption = caption;
        ParseMode = parseMode;
        Performer = performer;
        AudioDuration = audioDuration;
        ReplyMarkup = replyMarkup;
        InputMessageContent = inputMessageContent;
    }
    
}

public class InputMessageContent
{
    [JsonPropertyName("message_text")] 
    public string MessageText { get; init; }
    
    [JsonPropertyName("parse_mode")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; init; }
    
    [JsonPropertyName("disable_web_page_preview")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DisableWebPagePreview { get; init; }

    public InputMessageContent(string messageText, string? parseMode = "HTML", bool? disableWebPagePreview = true)
    {
        MessageText = messageText;
        ParseMode = parseMode;
        DisableWebPagePreview = disableWebPagePreview;
    }
}


public class InlineQueryResultButton
{
    [JsonPropertyName("start_parameter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StartParameter { get; init; }
    
    [JsonPropertyName("web_app")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WebApp { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; }

    public InlineQueryResultButton(string text, string? webApp = null, string? startParameter = null)
    {
        WebApp = webApp;
        StartParameter = startParameter;
        Text = text;
    }
}