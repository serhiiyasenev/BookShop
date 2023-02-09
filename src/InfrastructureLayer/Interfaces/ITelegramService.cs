using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace InfrastructureLayer.Interfaces
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string message);

        Task HandleMessageAsync(Update update);
    }
}
