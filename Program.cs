using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using NowPlaySharpBot.TelegramApi;
using NowPlaySharpBot.Database;
using NowPlaySharpBot.Spotify;
using NowPlaySharpBot.YouTubeDL;
using Npgsql;

namespace NowPlaySharpBot;

internal abstract class Program
{
    private static async Task Main()
    {
        var db = await Database.Database.Connect();
        Console.WriteLine(db);
        /*var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet("/login", (string code, string state) => "wau");

        var webTask = app.RunAsync("http://*:35139");*/
        var bot = new BotApi();
        bot.UpdateReceived += OnUpdate;
        var updateTask = bot.StartUpdatingAsync(db);
        Console.WriteLine("Bot is running, HALLELUJAH!");
        await updateTask;
    }
    
    private static async void OnUpdate(object? sender, UpdateEventArgs e)
    {
        var update = e.Update;
        var db = e.Database;
        
        if (update.Message != null)
        {
            var test = await Database.Database.CheckUser(db, update.Message.From.Id);
            Console.WriteLine(test);
            switch (update.Message.Text)
            {
                case "/start":
                {
                    var keyboard = new InlineKeyboard(
                        [
                            [
                                new Button("test", "test")
                            ],
                            [
                                new Button("test", url: "https://example.com")
                            ]
                        ]
                    );
                    await BotApi.SendMessage(update.Message.From.Id, "miao", keyboard: keyboard);
                    break;
                }
                case "/params":
                    await BotApi.SendMessage(update.Message.From.Id, "miao"); //test case without keyboard (dynamic params)
                    break;
                case "/audio":
                    await BotApi.SendAudio(update.Message.From.Id, "The Chainsmokers - Something Just Like This.mp3");
                    break;
                case "/youtubedl":
                    var audio = await YouTubeDL.YouTubeDL.Download("Tuttecose - Gazzelle");
                    await BotApi.SendAudio(update.Message.From.Id, audio);
                    File.Delete(audio);
                    break;
                case "/login":
                    var loginUrl = Spotify.Spotify.GenAuthUrl(Guid.NewGuid().ToString());
                    await BotApi.SendMessage(update.Message.From.Id, $"Well: {loginUrl}");
                    break;
            }
        }

        if (update.CallbackQuery != null)
        {
            switch (update.CallbackQuery.Data)
            {
                case "test":
                    await BotApi.EditMessageText(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId, "miao");
                    await BotApi.AnswerCallbackQuery(update.CallbackQuery.Id);
                    break;
            }
        }

        if (update.InlineQuery != null)
        {
            Console.WriteLine(update.InlineQuery.Query);
            //The Chainsmokers - Something Just Like This.mp3
            if (update.InlineQuery.Query == "chainsmokers")
            {
                var audio = new InlineQueryResultCachedAudio("audio", "chainsmokers", "CQACAgQAAxkDAAIZJWV6MOxYAhCWhUs1v8v46_qH18tDAALHEgACzq7QUzg6I1_VaUCJMwQ");
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [audio]);
            }
            else
            {
                var article = new InlineQueryResultArticle("article", "rockanro", "lesgo", new InputMessageContent("input message content"), null, null, false, "description");
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [article]);
            }
        }
        
    }
}