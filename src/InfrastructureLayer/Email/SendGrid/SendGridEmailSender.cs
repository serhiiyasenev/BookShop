﻿using InfrastructureLayer.Email.Interfaces;
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
        private readonly SendGridSettings _options;
        private readonly SendGridClient _client;

        public SendGridEmailSender(IOptions<SendGridSettings> options)
        {
            _options = options.Value;
            _emailFrom = Environment.GetEnvironmentVariable(_options.SenderEmailFromKey);
            _client = new SendGridClient(Environment.GetEnvironmentVariable(_options.ApiKey));
        }

        public async Task<(bool, string)> SendEmailAsync(string emailTo, string subject, string message)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_emailFrom),
                Subject = subject,
                PlainTextContent = StripHtmlTags(message),
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(emailTo));

            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // disable tracking settings
            // ref.: https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            var result = await _client.SendEmailAsync(msg);

            return (result.IsSuccessStatusCode, await result.Body.ReadAsStringAsync());
        }

        private static string StripHtmlTags(string html)
        {
            var regex = new Regex("<[^>]+>", RegexOptions.Compiled);
            return regex.Replace(html, string.Empty);
        }
    }
}