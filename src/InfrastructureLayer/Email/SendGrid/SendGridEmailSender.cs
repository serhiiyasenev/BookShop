using InfrastructureLayer.Email.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfrastructureLayer.Email.SendGrid
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _emailFrom;
        private readonly string _nameFrom;
        private readonly SendGridSettings _options;
        private readonly SendGridClient _client;

        public SendGridEmailSender(IOptions<SendGridSettings> options)
        {
            _options = options.Value;
            _nameFrom = _options.SenderNameFrom;
            _emailFrom = Environment.GetEnvironmentVariable(_options.SenderEmailFromKey);
            _client = new SendGridClient(Environment.GetEnvironmentVariable(_options.ApiKey));
        }

        public async Task<(bool, string)> SendEmailAsync(string emailTo, string subject, string message)
        {
            try
            {
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(_emailFrom, _nameFrom),
                    Subject = subject,
                    PlainTextContent = StripHtmlTags(message),
                    HtmlContent = message
                };
                msg.AddTo(new EmailAddress(emailTo));

                // disable tracking settings
                // ref: https://sendgrid.com/docs/User_Guide/Settings/tracking.html
                msg.SetClickTracking(false, false);
                msg.SetOpenTracking(false);
                msg.SetGoogleAnalytics(false);
                msg.SetSubscriptionTracking(false);

                var result = await _client.SendEmailAsync(msg);

                return (result.IsSuccessStatusCode, await result.Body.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static string StripHtmlTags(string html)
        {
            var regex = new Regex("<[^>]+>", RegexOptions.Compiled);
            return regex.Replace(html, string.Empty);
        }
    }
}
