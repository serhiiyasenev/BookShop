using System.Threading.Tasks;

namespace InfrastructureLayer.Email.Interfaces
{
    public interface IEmailSender
    {
        /// <summary>
        /// SendEmailAsync via connected provider <br/>
        /// if IsSendStatusSuccess is false we should check ResponseBody message and handle it
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns>IsSendStatusSuccess and ResponseBody </returns>
        Task<(bool, string)> SendEmailAsync(string emailTo, string subject, string message);
    }
}
