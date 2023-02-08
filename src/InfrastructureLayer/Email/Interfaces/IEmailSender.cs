using System.Threading.Tasks;

namespace InfrastructureLayer.Email.Interfaces
{
    public interface IEmailSender
    {
        Task<(bool, string)> SendEmailAsync(string emailTo, string subject, string message);
    }
}
