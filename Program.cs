using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
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
        
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(builder.Environment.ContentRootPath, "files")),
            RequestPath = "/files",
            EnableDirectoryBrowsing = false
        });

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

            string? loginUrl;
            string? state;
            InlineKeyboard? keyboard;
            switch (update.Message.Text)
            {
                case "/start":
                {
                    keyboard = new InlineKeyboard(
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
                case "/start login":
                    state = Convert.ToBase64String(BitConverter.GetBytes(update.Message.From.Id));
                    loginUrl = Spotify.Spotify.GenAuthUrl(state);
                    keyboard = new InlineKeyboard(
                        [
                            [
                                new Button("Login", url: loginUrl)
                            ]
                        ]
                    );
                    await BotApi.SendMessage(update.Message.From.Id, "Click on the button below to login:", keyboard: keyboard);
                    break;
                case "/login":
                    state = Convert.ToBase64String(BitConverter.GetBytes(update.Message.From.Id));
                    loginUrl = Spotify.Spotify.GenAuthUrl(state);
                    keyboard = new InlineKeyboard(
                        [
                            [
                                new Button("Login", url: loginUrl)
                            ]
                        ]
                    );
                    await BotApi.SendMessage(update.Message.From.Id, "Click on the button to perform the travasation of your personal data to my servers:");
                    break;
                case "/empty":
                    var empty = await BotApi.SendAudio(update.Message.From.Id, "empty.mp3");
                    Console.WriteLine(empty.Dump());
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
                var defaultButton = new InlineQueryResultButton("Login first!", null, "login");
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [], defaultButton);
            }
            else
            {
                List<object> resultsList = [];
                var keyboard = new InlineKeyboard(
                    [
                        [
                            new Button("Loading...", callbackData: "loading")
                        ]
                    ]
                );
                var currentlyPlaying = await Spotify.Spotify.GetCurrentlyPlaying(tokens[0].ToString());
                if (currentlyPlaying.Error != null & currentlyPlaying.Error?.Status == 401)
                {
                    var refreshToken = await Spotify.Spotify.RefreshToken(tokens[1].ToString());
                    await Database.Database.UpdateAccessToken(db, refreshToken, update.InlineQuery.From.Id);
                    currentlyPlaying = await Spotify.Spotify.GetCurrentlyPlaying(tokens[0].ToString());
                }
                var audio = (await Database.Database.GetMusic(db, currentlyPlaying.Item?.Id)) ?? currentlyPlaying.Item?.PreviewUrl ?? $"http://{Util.GetLocalIPAddress()}:35139/files/empty.mp3";
                var currentlyPlayingResult = new InlineQueryResultAudio("audio", currentlyPlaying.Item?.Id, audio, currentlyPlaying.Item?.Name, string.Join(", ", currentlyPlaying.Item.Artists.Select(artist => artist.Name)), null, "Downloading the song...", keyboard);
                resultsList.Add(currentlyPlayingResult);
                var recentlyPlayed = await Spotify.Spotify.GetRecentlyPlayed(tokens[0].ToString());
                if (recentlyPlayed.Items != null)
                {
                    foreach (var item in recentlyPlayed.Items)
                    {
                        var music = await Database.Database.GetMusic(db, item.Item.Id) ?? item.Item.PreviewUrl ?? $"http://{Util.GetLocalIPAddress()}:35139/files/empty.mp3";

                        var result = new InlineQueryResultAudio(
                            "audio",
                            item.Item.Id,
                            music,
                            item.Item.Name,
                            string.Join(", ", item.Item.Artists.Select(artist => artist.Name)),
                            null,
                            (music == null) ? "Downloading the song..." : null,
                            (music == null) ? keyboard : null
                        );
                        resultsList.Add(result);
                    }
                }
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, resultsList);
            }
        }

        if (update.ChosenInlineResult != null)
        {
            var tokens = await Database.Database.GetTokens(db, update.ChosenInlineResult.From.Id);
            var track = await Spotify.Spotify.GetTrack(update.ChosenInlineResult.ResultId, tokens[0].ToString());
            if (track.Error == null)
            {
                if (!string.IsNullOrEmpty(update.ChosenInlineResult.InlineMessageId))
                {
                    var audioName = await YouTubeDL.YouTubeDL.Download($"{track.Name} - {track.Artists[0].Name} {track.Album.Name}", track.Id);
                    var audioSent = await BotApi.SendAudio(-1002012184102, audioName, track.Album.Images[0].Url, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), (track.DurationMs / 1000) ?? 0);
                    var audio = new InputMediaAudio("audio", audioSent.Result.Audio.FileId, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), track.Album.Images[0].Url, (track.DurationMs / 1000));
                    await BotApi.EditMessageMedia(update.ChosenInlineResult.InlineMessageId, audio);
                    await Database.Database.AddMusic(db, track.Id, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), track.Album.Name, track.Album.Images[0].Url, audioSent.Result.Audio.FileId);
                    File.Delete(audioName);
                }
            }
        }
        
    }
}