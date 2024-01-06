using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using NowPlaySharpBot;
using NowPlaySharpBot.Database;
using NowPlaySharpBot.Spotify;
using NowPlaySharpBot.TelegramApi;
using NowPlaySharpBot.YouTubeDL;

var db = new Database();
var builder = WebApplication.CreateBuilder();
var app = builder.Build();
var bot = new BotApi();
var spotify = new SpotifyWrap(db);

var callbackUrl = Environment.GetEnvironmentVariable("CALLBACK_URL");

app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "files")),
    RequestPath = "/files",
    EnableDirectoryBrowsing = false
});

app.MapGet("/login", async (string code, string state) => (await SpotifyWrap.Login(code, state)));

var webTask = app.RunAsync("http://*:35139");
bot.UpdateReceived += OnUpdate;
var updateTask = bot.StartUpdatingAsync(db.Db);
Console.WriteLine("Bot is running, HALLELUJAH!");
await Task.WhenAll(webTask, updateTask);

async void OnUpdate(object? sender, UpdateEventArgs e)
    {
        var update = e.Update;
        var db = e.Database;
        
        if (update.Message != null)
        {
            var checkUser = await Database.CheckUser(db, update.Message.From.Id);
            if (checkUser == 0)
            {
                await Database.AddUser(db, update.Message.From);
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
                    loginUrl = spotify.GenAuthUrl(state);
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
                    loginUrl = spotify.GenAuthUrl(state);
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

        List<object>? tokens;
        if (update.InlineQuery != null)
        {
            var defaultButton = new InlineQueryResultButton("Login first!", null, "login");
            tokens = await Database.GetTokens(db, update.InlineQuery.From.Id);
            if (tokens.Count == 0)
            {
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [], defaultButton);
            }
            else
            {
                List<InlineQueryResultAudio> resultsList = new List<InlineQueryResultAudio>();
                var keyboard = new InlineKeyboard(
                    [
                        [
                            new Button("Loading...", callbackData: "loading")
                        ]
                    ]
                );
                var currentlyPlaying = await SpotifyWrap.GetCurrentlyPlaying(tokens[0].ToString(), update.InlineQuery.From.Id);
                if (currentlyPlaying == null)
                {
                    await BotApi.AnswerInlineQuery(update.InlineQuery.Id, [], defaultButton);
                    return;
                }
                
                if (currentlyPlaying.Item != null){
                    var audio = currentlyPlaying.Item != null ? await Database.GetMusic(db, currentlyPlaying.Item.Id) : null;
                    var currentlyPlayingResult = new InlineQueryResultAudio("audio", currentlyPlaying.Item?.Id, audio ?? currentlyPlaying.Item?.PreviewUrl ?? $"{callbackUrl}/files/empty.mp3", currentlyPlaying.Item?.Name, string.Join(", ", currentlyPlaying.Item.Artists.Select(artist => artist.Name)), null, (audio == null) ? "Downloading the song..." : null, (audio == null) ? keyboard : null);
                    resultsList.Add(currentlyPlayingResult);
                }

                var recentlyPlayed = await SpotifyWrap.GetRecentlyPlayed(tokens[0].ToString());
                if (recentlyPlayed.Items != null)
                {
                    foreach (var item in recentlyPlayed.Items)
                    {
                        var music = await Database.GetMusic(db, item.Item.Id);

                        var result = new InlineQueryResultAudio(
                            "audio",
                            item.Item.Id,
                            music ?? item.Item.PreviewUrl ?? $"{callbackUrl}/files/empty.mp3",
                            item.Item.Name,
                            string.Join(", ", item.Item.Artists.Select(artist => artist.Name)),
                            null,
                            (music == null) ? "Downloading the song..." : null,
                            (music == null) ? keyboard : null
                        );
                        resultsList.Add(result);
                    }
                }
                var distinctResultsList = resultsList.Distinct(InlineQueryResultAudio.IdComparer).ToList();
                await BotApi.AnswerInlineQuery(update.InlineQuery.Id, distinctResultsList);
            }
        }

        if (update.ChosenInlineResult == null) return;
        
            tokens = await Database.GetTokens(db, update.ChosenInlineResult.From.Id);
            var track = await SpotifyWrap.GetTrack(update.ChosenInlineResult.ResultId, tokens[0].ToString());
            if (track.Error != null) return;
            {
                if (string.IsNullOrEmpty(update.ChosenInlineResult.InlineMessageId)) return;
                var audioName = await YouTubeDL.Download($"{track.Artists[0].Name} - {track.Name} ({track.Album.Name})", track.Id);
                var audioSent = await BotApi.SendAudio(-1002012184102, audioName, track.Album.Images[0].Url, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), (track.DurationMs / 1000) ?? 0);
                var audio = new InputMediaAudio("audio", audioSent.Result.Audio.FileId, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), track.Album.Images[0].Url, (track.DurationMs / 1000));
                await BotApi.EditMessageMedia(update.ChosenInlineResult.InlineMessageId, audio);
                await Database.AddMusic(db, track.Id, track.Name, string.Join(", ", track.Artists.Select(artist => artist.Name)), track.Album.Name, track.Album.Images[0].Url, audioSent.Result.Audio.FileId);
                File.Delete(audioName);
            }

    }