﻿namespace NowPlaySharpBot;

internal abstract class Program
{
    private static async Task Main()
    {
        var bot = new BotApi();
        bot.UpdateReceived += OnUpdate;
        var updateTask = bot.StartUpdatingAsync();
        Console.WriteLine("Bot is running, HALLELUJAH!");
        await updateTask;
    }
    private static async void OnUpdate(object? sender, UpdateEventArgs e)
    {
        var update = e.Update;
        if (update.Message != null)
        {
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