﻿using Microsoft.Extensions.Hosting;
using VoiceTexterBot.Controllers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VoiceTexterBot;

internal class Bot : BackgroundService
{
    private ITelegramBotClient _telegramClient;
    private InlineKeyboardController _inlineKeyboardController;
    private TextMessageController _textMessageController;
    private DefaultMessageController _defaultMessageController;

    public Bot(ITelegramBotClient telegramClient,
               InlineKeyboardController inlineKeyboardController,
               TextMessageController textMessageController,
               DefaultMessageController defaultMessageController)
    {
        _telegramClient = telegramClient;
        _inlineKeyboardController = inlineKeyboardController;
        _textMessageController = textMessageController;
        _defaultMessageController = defaultMessageController;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _telegramClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions() { AllowedUpdates = { } }, // Здесь выбираем, какие обновления хотим получать. В данном случае разрешены все
            cancellationToken: stoppingToken);

        Console.WriteLine("Бот запущен");
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        //  Обрабатываем нажатия на кнопки  из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
        if (update.Type == UpdateType.CallbackQuery)
        {
            await _inlineKeyboardController.Handle(update.CallbackQuery, cancellationToken);
            return;
        }

        // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
        if (update.Type == UpdateType.Message)
        {
            switch (update.Message!.Type)
            {
                case MessageType.Text:
                    if (update.Message!.Text != "/start")
                    {
                        var userButtonCode = this._inlineKeyboardController._memoryStorage.GetSession(update.Message.Chat.Id).ButtonCode;
                        if (userButtonCode == "calcchar")
                            update.Message.Text = $"в вашем сообщении {update.Message.Text.Length} символов";
                        if (userButtonCode == "calcsum")
                        {
                            try
                            {
                                string[] value = update.Message.Text.Split(' ');
                                int sum = 0;
                                sum = value.Sum(x => Convert.ToInt32(x));
                                update.Message.Text = $"Сумма чисел : {sum}";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }

                    await _textMessageController.Handle(update.Message, cancellationToken);
                    return;
                default:
                    await _defaultMessageController.Handle(update.Message, cancellationToken);
                    return;
            }
        }
    }

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Задаем сообщение об ошибке в зависимости от того, какая именно ошибка произошла
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        // Выводим в консоль информацию об ошибке
        Console.WriteLine(errorMessage);

        // Задержка перед повторным подключением
        Console.WriteLine("Ожидаем 10 секунд перед повторным подключением.");
        Thread.Sleep(10000);

        return Task.CompletedTask;
    }
}