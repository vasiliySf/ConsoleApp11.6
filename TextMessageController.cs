using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using VoiceTexterBot;
using VoiceTexterBot.Services;

namespace VoiceTexterBot.Controllers
{
    public class TextMessageController
    {
        private readonly ITelegramBotClient _telegramClient;

        public TextMessageController(ITelegramBotClient telegramBotClient)
        {            
               _telegramClient = telegramBotClient;
        }

        public async Task Handle(Message message, CancellationToken ct)
        {
            switch (message.Text)
            {
                case "/start":

                    // Объект, представляющий кноки
                    var buttons = new List<InlineKeyboardButton[]>();
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"подсчёт количества символов в тексте" , $"calcchar"),
                        InlineKeyboardButton.WithCallbackData($"вычисление суммы чисел" , $"calcsum")
                    });

                    // передаем кнопки вместе с сообщением (параметр ReplyMarkup)
                    await _telegramClient.SendTextMessageAsync(message.Chat.Id, $"<b>  Наш бот делает подсчет символов в тексте.</b> {Environment.NewLine}" +
                        $"{Environment.NewLine}Можно считать суммы чисел.{Environment.NewLine}", cancellationToken: ct, parseMode: ParseMode.Html, 
                        replyMarkup: new InlineKeyboardMarkup(buttons));

                    break;
                default:
                    //var userButtonCode = _memoryStorage.GetSession(message.Chat.Id).ButtonCode;
                   
                    await _telegramClient.SendTextMessageAsync(message.Chat.Id,  message.Text, cancellationToken: ct);
                    //await _telegramClient.SendTextMessageAsync(message.Chat.Id, $"Длина сообщения: {message.Text.Length} знаков", cancellationToken: ct);
                    break;
            }
        }
    }
}