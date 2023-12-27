using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace NowPlaySharpBot;

// https://core.telegram.org/bots/api#available-types

public record Response<T>
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("result")] public T? Result { get; init; }
    
    // error handling
    [JsonPropertyName("error_code")] public int? ErrorCode { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
}

public record Result
{
    // sendMessage
    [JsonPropertyName("message_id")] public int MessageId { get; init; }
    [JsonPropertyName("from")] public From? From { get; init; }
    [JsonPropertyName("chat")] public Chat? Chat { get; init; }
    [JsonPropertyName("date")] public int? Date { get; init; }
    [JsonPropertyName("text")] public string? Text { get; init; }
    
    // sendAudio
    [JsonPropertyName("audio")] public Audio? Audio { get; init; }
    
    // getMe
    [JsonPropertyName("can_join_groups")] public bool? CanJoinGroups { get; init; }
    [JsonPropertyName("can_read_all_group_messages")] public bool? CanReadAllMessages { get; init; }
    [JsonPropertyName("supports_inline_queries")] public bool? SupportsInlineQueries { get; init; }
}

public record From
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("is_bot")] public bool IsBot { get; init; }
    [JsonPropertyName("first_name")] public required string FirstName { get; init; }
    [JsonPropertyName("last_name")] public string? LastName { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
}

public record Chat
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("first_name")] public required string FirstName { get; init; }
    [JsonPropertyName("last_name")] public string? LastName { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
    [JsonPropertyName("type")] public required string Type { get; init; }
}

public record Audio
{
    [JsonPropertyName("file_id")] public required string FileId { get; init; }
    [JsonPropertyName("file_unique_id")] public required string FileUniqueId { get; init; }
    [JsonPropertyName("duration")] public int Id { get; init; }
    [JsonPropertyName("performer")] public string? Performer { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("file_name")] public string? FileName { get; init; }
    [JsonPropertyName("mime_type")] public string? MimeType { get; init; }
    //[JsonPropertyName("thumbnail")] public PhotoSize? Thumbnail { get; init; } (it doesn't glue me and I don't need it)
}