using InfrastructureLayer.Interfaces;
using InfrastructureLayer.Telegram;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgBot.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        private readonly long _chatId;
        private readonly TelegramSettings _options;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramService(IOptions<TelegramSettings> options)
        {
            _options = options.Value;
            _chatId = long.Parse(Environment.GetEnvironmentVariable(_options.ChatIdKey));
            _telegramBotClient = new TelegramBotClient(Environment.GetEnvironmentVariable(_options.BotAccessTokenKey));
            _telegramBotClient.StartReceiving(Update, Error);
        }

        public async Task HandleMessageAsync(Update update)
        {
            try
            {
                var query = update.Message.Text;

                if (update.Type != UpdateType.Message)
                {
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"I cannot handle {update.Type}");
                    return;
                }

                if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"Hi there! {update.Message.Chat.FirstName} 😜");
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telegram Bot HandleMessage Exception: '{ex.Message}'");
            }
        }

        public Task SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        private Task Update(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}
