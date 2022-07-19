using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoiceTexterBot.Controllers;
using VoiceTexterBot.Services;

namespace VoiceTexterBot.Controllers
{
    public class InlineKeyboardController
    {
        public readonly IStorage _memoryStorage;
        private readonly ITelegramBotClient _telegramClient;

        public InlineKeyboardController(ITelegramBotClient telegramBotClient, IStorage memoryStorage)//
        {
            _telegramClient = telegramBotClient;
            _memoryStorage = memoryStorage;
        }

        public async Task Handle(CallbackQuery? callbackQuery, CancellationToken ct)
        {
            if (callbackQuery?.Data == null)
                return;

            // Обновление пользовательской сессии новыми данными
            _memoryStorage.GetSession(callbackQuery.From.Id).ButtonCode = callbackQuery.Data;

            // Генерим информационное сообщение           
            string CaptureText = callbackQuery.Data switch
            {
                "calcchar" => " подсчёт количества символов в тексте",
                "calcsum" => " вычисление суммы чисел",
                _ => String.Empty
            };
                   

            // Отправляем в ответ уведомление о выборе
              await _telegramClient.SendTextMessageAsync(callbackQuery.From.Id,
                $"<b>Кнопка - {CaptureText}.{Environment.NewLine}</b>" +
                $"{Environment.NewLine}Можно поменять в главном меню.", cancellationToken: ct, parseMode: ParseMode.Html);
        }
    }
}