using System.Text.Json.Serialization;

namespace NowPlaySharpBot;

// https://core.telegram.org/bots/api#available-types

public class InlineKeyboard
{
    [JsonPropertyName("inline_keyboard")]
    public List<List<Button>> Keyboard { get; init; }

    public InlineKeyboard(List<List<Button>> keyboard)
    {
        Keyboard = keyboard;
    }
}

public class Button
{

    [JsonPropertyName("callback_data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CallbackData { get; init; }
    
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; }

    public Button(string text, string? callbackData = null, string? url = null)
    {
        if ((url != null && callbackData != null) || (url == null && callbackData == null))
        {
            throw new ArgumentException("Either url or callback_data must be provided, but not both.");
        }

        CallbackData = callbackData;
        Url = url;
        Text = text;
    }
}
