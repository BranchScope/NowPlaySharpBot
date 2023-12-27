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
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet("/login", async (string code, string state) => (await Spotify.Spotify.Login(code, state, db)));

        var webTask = app.RunAsync("http://*:35139");
        var bot = new BotApi();
        bot.UpdateReceived += OnUpdate;
        var updateTask = bot.StartUpdatingAsync(db);
        Console.WriteLine("Bot is running, HALLELUJAH!");
        await Task.WhenAll(webTask, updateTask);
    }
    
    private static async void OnUpdate(object? sender, UpdateEventArgs e)
    {
        var update = e.Update;
        var db = e.Database;
        
        if (update.Message != null)
        {
            var test = await Database.Database.CheckUser(db, update.Message.From.Id);
            Console.WriteLine(test);
            if (test == 0)
            {
                var a = await Database.Database.AddUser(db, update.Message.From);
                Console.WriteLine(a);
            }
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
                    var state = Convert.ToBase64String(BitConverter.GetBytes(update.Message.From.Id));
                    var loginUrl = Spotify.Spotify.GenAuthUrl(Guid.NewGuid().ToString());
                    await BotApi.SendMessage(update.Message.From.Id, $"Well: {loginUrl}");
                    break;
                case "/empty":
                    var empty = await BotApi.SendAudio(update.Message.From.Id, "empty.mp3");
                    Console.WriteLine(empty.Dump());
                    break;
                case "/gettokens":
                    var tokens = await Database.Database.GetTokens(db, update.Message.From.Id);
                    Console.WriteLine(tokens.Dump());
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
            var tokens = await Database.Database.GetTokens(db, update.InlineQuery.From.Id);
            if (tokens.Count == 0)
            {
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [], new InlineQueryResultButton("you could stay under my umbrella ella ella eh eh eh", null, "help"));
            }
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