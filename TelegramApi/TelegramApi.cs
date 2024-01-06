using System.Text.Json;
using Npgsql;
using RestSharp;

namespace NowPlaySharpBot.TelegramApi;

public sealed class BotApi
{
    private const string Resource = "https://api.telegram.org/bot";
    private static readonly string? Token = Environment.GetEnvironmentVariable("TOKEN");

    private static readonly RestClientOptions Options = new RestClientOptions(Resource + Token) {
        ThrowOnAnyError = false,
        ThrowOnDeserializationError = false
    };
    private static readonly RestClient Client = new RestClient(options: Options);
    public event EventHandler<UpdateEventArgs>? UpdateReceived;

    public async Task StartUpdatingAsync(NpgsqlConnection db)
    {
        await BotApi.GetUpdates(-1, 1);
        var offset = 0;
        while (true)
        {
            var updateResponse = await BotApi.GetUpdates(offset, 1);
            if (updateResponse.Ok != true) continue;
            if (!(updateResponse.Result?.Count > 0)) continue;
            var update = updateResponse.Result[^1];
            offset = update.UpdateId + 1;
            
            OnUpdateReceived(db, update);
        }
    }

    private void OnUpdateReceived(NpgsqlConnection db, Update update)
    {
        UpdateReceived?.Invoke(this, new UpdateEventArgs(db, update));
    }

    // https://core.telegram.org/bots/api#getupdates
    public static async Task<UpdateResponse> GetUpdates(int? offset = 0, int? limit = 100, int? timeout = 10, List<string>? allowedUpdates = null)
    {
        var request = new RestRequest("getUpdates", Method.Post);
        var param = new
        {
            offset,
            limit,
            timeout,
            allowedUpdates
        };
        
        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<UpdateResponse>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }

    // https://core.telegram.org/bots/api#sendmessage
    public static async Task<Response<Result>> SendMessage(long chatId, string text, InlineKeyboard? keyboard = null, bool disableWebPagePreview = true)
    {
        var request = new RestRequest("sendMessage", Method.Post);
        var param = new Dictionary<string, object>
        {
            { "chat_id", chatId },
            { "text", text },
            { "disable_web_page_preview", disableWebPagePreview },
            { "parse_mode", "HTML" }
        };

        if (keyboard != null)
        {
            param.Add("reply_markup", JsonSerializer.Serialize(keyboard));
        }

        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<Result>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    // https://core.telegram.org/bots/api#sendaudio
    public static async Task<Response<Result>> SendAudio(long chatId, string audio, string? thumbnail = null, string? title = null, string? performer = null, int duration = 0, string? caption = null, InlineKeyboard? keyboard = null)
    {
        var request = new RestRequest("sendAudio", Method.Post);
        
        request.AddHeader("Content-Type", "multipart/form-data");
        request.AddParameter("chat_id", chatId);
        request.AddParameter("caption", caption);
        request.AddParameter("parse_mode", "HTML");
        request.AddFile("audio", audio);
        
        if (thumbnail != null)
        {
            request.AddParameter("thumbnail", thumbnail);
        }

        if (title != null)
        {
            request.AddParameter("title", title);
        }
        
        if (performer != null)
        {
            request.AddParameter("performer", performer);
        }
        
        if (duration != null)
        {
            request.AddParameter("duration", duration);
        }

        if (keyboard != null)
        {
            request.AddParameter("reply_markup", JsonSerializer.Serialize(keyboard));
        }
        
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<Result>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    // https://core.telegram.org/bots/api#editmessagetext
    public static async Task<Response<Result>> EditMessageText(long chatId, int messageId, string text, InlineKeyboard? keyboard = null, bool disableWebPagePreview = true)
    {
        var request = new RestRequest("editMessageText", Method.Post);
        var param = new Dictionary<string, object>
        {
            { "chat_id", chatId },
            { "message_id", messageId },
            { "text", text },
            { "disable_web_page_preview", disableWebPagePreview },
            { "parse_mode", "HTML" }
        };

        if (keyboard != null)
        {
            param.Add("reply_markup", JsonSerializer.Serialize(keyboard));
        }
        
        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<Result>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    // https://core.telegram.org/bots/api#editmessagemedia
    public static async Task<Response<bool>> EditMessageMedia(string inlineMessageId, InputMediaAudio audio, InlineKeyboard? keyboard = null, bool disableWebPagePreview = true)
    {
        var request = new RestRequest("editMessageMedia", Method.Post);
        var param = new Dictionary<string, object>
        {
            { "inline_message_id", inlineMessageId },
            { "media", JsonSerializer.Serialize(audio) },
            { "disable_web_page_preview", disableWebPagePreview },
            { "parse_mode", "HTML" }
        };

        if (keyboard != null)
        {
            param.Add("reply_markup", JsonSerializer.Serialize(keyboard));
        }
        
        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<bool>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }

    // https://core.telegram.org/bots/api#answercallbackquery
    public static async Task<Response<bool>> AnswerCallbackQuery(string callbackQueryId, int cacheTime = 0, bool showAlert = false, string? text = null, string? url = null)
    {
        var request = new RestRequest("answerCallbackQuery", Method.Post);
        var param = new Dictionary<string, object>
        {
            { "callback_query_id", callbackQueryId },
            { "cache_time", cacheTime },
            { "show_alert", showAlert }
        };

        if (text != null)
        {
            param.Add("text", text);
        }

        if (url != null)
        {
            param.Add("url", url);
        }
        
        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<bool>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
    
    // https://core.telegram.org/bots/api#answerinlinequery
    public static async Task<Response<bool>> AnswerInlineQuery(string inlineQueryId, List<InlineQueryResultAudio> results, InlineQueryResultButton? button = null, int cacheTime = 0)
    {
        var request = new RestRequest("answerInlineQuery", Method.Post);
        var param = new Dictionary<string, object>
        {
            { "inline_query_id", inlineQueryId },
            { "results", results },
            { "cache_time", cacheTime }
        };

        if (button != null)
        {
            param.Add("button", JsonSerializer.Serialize(button));
        }

        request.AddJsonBody(param);
        var response = await Client.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<Response<bool>>(response.Content ?? throw new MissingFieldException()) ?? throw new Exception("wtf!?");
    }
}