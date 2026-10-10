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
        private readonly ISendGridClient _client;

        public SendGridEmailSender(IOptions<SendGridSettings> options)
            : this(options, new SendGridClient(Environment.GetEnvironmentVariable(options.Value.ApiKey)))
        {
        }

        public SendGridEmailSender(IOptions<SendGridSettings> options, ISendGridClient client)
        {
            var settings = options.Value;
            _nameFrom = settings.SenderNameFrom;
            _emailFrom = Environment.GetEnvironmentVariable(settings.SenderEmailFromKey) ?? "default@email.com";
            _client = client ?? throw new ArgumentNullException(nameof(client));
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

