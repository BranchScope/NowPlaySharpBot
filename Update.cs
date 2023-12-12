using System.Text.Json.Serialization;

namespace NowPlaySharpBot;

//Update handler object
public class UpdateEventArgs : EventArgs
{
    public Update Update { get; }

    public UpdateEventArgs(Update update)
    {
        Update = update;
    }
}

//https://core.telegram.org/bots/api#available-types
//Those records are missing many stuffs, I don't need it.

public record UpdateResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("result")] public List<Update>? Result { get; init; }
}
public record Update
{
    [JsonPropertyName("update_id")] public int UpdateId { get; init; }
    [JsonPropertyName("message")] public Message? Message { get; init; }
    [JsonPropertyName("edited_message")] public Message? EditedMessage { get; init; }
    [JsonPropertyName("callback_query")] public CallbackQuery? CallbackQuery { get; init; }
    [JsonPropertyName("inline_query")] public InlineQuery? InlineQuery { get; init; }
    [JsonPropertyName("chosen_inline_result")] public ChosenInlineResult? ChosenInlineResult { get; init; }
}

public record Message
{
    [JsonPropertyName("message_id")] public int MessageId { get; init; }
    [JsonPropertyName("from")] public User From { get; init; }
    [JsonPropertyName("sender_chat")] public Chat? SenderChat { get; init; }
    [JsonPropertyName("date")] public int? Date { get; init; }
    [JsonPropertyName("chat")] public required Chat Chat { get; init; }
    [JsonPropertyName("forward_from")] public User? ForwardFrom { get; init; }
    [JsonPropertyName("forward_from_chat")] public Chat? ForwardFromChat { get; init; }
    [JsonPropertyName("forward_from_message_id")] public int? ForwardFromMessageId { get; init; }
    [JsonPropertyName("reply_to_message")] public Message? ReplyToMessage { get; init; }
    [JsonPropertyName("text")] public string? Text { get; init; }
}

public record User
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("is_bot")] public bool IsBot { get; init; }
    [JsonPropertyName("first_name")] public required string FirstName { get; init; }
    [JsonPropertyName("last_name")] public string? LastName { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
    [JsonPropertyName("language_code")] public string? LanguageCode { get; init; }
    [JsonPropertyName("is_premium")] public bool? IsPremium { get; init; }
}

public record CallbackQuery
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("from")] public User? From { get; init; }
    [JsonPropertyName("message")] public Message? Message { get; init; }
    [JsonPropertyName("inline_message_id")] public string? InlineMessageId { get; init; }
    [JsonPropertyName("chat_instance")] public string? ChatInstance { get; init; }
    [JsonPropertyName("data")] public string? Data { get; init; }
}

public record InlineQuery
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("from")] public User From { get; init; }
    [JsonPropertyName("query")] public required string Query { get; init; }
    [JsonPropertyName("offset")] public required string Offset { get; init; }
    [JsonPropertyName("chat_type")] public string? ChatType { get; init; }
}

public record ChosenInlineResult
{
    [JsonPropertyName("result_id")] public required string ResultId { get; init; }
    [JsonPropertyName("from")] public required User From { get; init; }
    [JsonPropertyName("inline_message_id")] public string? InlineMessageId { get; init; }
    [JsonPropertyName("query")] public required string Query { get; init; }
}