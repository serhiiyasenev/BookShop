using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfrastructureLayer.Email.SendGrid;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace UnitTests.Infrastructure
{
    public class SendGridEmailSenderTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task SendEmail_BuildsBothBodiesAndDisablesTracking(bool configuredSender)
        {
            using var senderVariable = new TestEnvironmentVariable(configuredSender ? "books@example.com" : null);
            var options = Settings(senderVariable.Name);
            var client = new Mock<ISendGridClient>(MockBehavior.Strict);
            SendGridMessage sent = null;
            using var body = new StringContent("accepted");
            client.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .Callback<SendGridMessage, CancellationToken>((message, _) => sent = message)
                .ReturnsAsync(new Response(HttpStatusCode.Accepted, body, null));
            var sender = new SendGridEmailSender(options, client.Object);

            var result = await sender.SendEmailAsync("reader@example.com", "Booking confirmed", "<h1>Booked</h1><p>Two books</p>");

            Assert.That(result, Is.EqualTo((true, "accepted")));
            Assert.That(sent.From.Email, Is.EqualTo(configuredSender ? "books@example.com" : "default@email.com"));
            Assert.That(sent.From.Name, Is.EqualTo("BookShop"));
            Assert.That(sent.Personalizations.Single().Tos.Single().Email, Is.EqualTo("reader@example.com"));
            Assert.That(sent.Subject, Is.EqualTo("Booking confirmed"));
            Assert.That(sent.HtmlContent, Is.EqualTo("<h1>Booked</h1><p>Two books</p>"));
            Assert.That(sent.PlainTextContent, Is.EqualTo("BookedTwo books"));
            Assert.That(sent.TrackingSettings.ClickTracking.Enable, Is.False);
            Assert.That(sent.TrackingSettings.ClickTracking.EnableText, Is.False);
            Assert.That(sent.TrackingSettings.OpenTracking.Enable, Is.False);
            Assert.That(sent.TrackingSettings.Ganalytics.Enable, Is.False);
            Assert.That(sent.TrackingSettings.SubscriptionTracking.Enable, Is.False);
            client.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendEmail_ReturnsProviderRejectionBody()
        {
            using var senderVariable = new TestEnvironmentVariable("books@example.com");
            using var body = new StringContent("sender identity is not verified");
            var client = new Mock<ISendGridClient>();
            client.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.Forbidden, body, null));

            var result = await new SendGridEmailSender(Settings(senderVariable.Name), client.Object)
                .SendEmailAsync("reader@example.com", "Booking", "Plain text");

            Assert.That(result, Is.EqualTo((false, "sender identity is not verified")));
        }

        [Test]
        public async Task SendEmail_ReturnsTransportFailure()
        {
            using var senderVariable = new TestEnvironmentVariable("books@example.com");
            var client = new Mock<ISendGridClient>();
            client.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Provider unavailable"));

            var result = await new SendGridEmailSender(Settings(senderVariable.Name), client.Object)
                .SendEmailAsync("reader@example.com", "Booking", "Message");

            Assert.That(result, Is.EqualTo((false, "Provider unavailable")));
        }

        [Test]
        public void Constructor_CanCreateDefaultClientFromConfiguration()
        {
            using var senderVariable = new TestEnvironmentVariable("books@example.com");
            using var apiKey = new TestEnvironmentVariable("SG.unit-test-placeholder");
            var options = Settings(senderVariable.Name);
            options.Value.ApiKey = apiKey.Name;

            Assert.That(new SendGridEmailSender(options), Is.Not.Null);
        }

        [Test]
        public void Constructor_RejectsMissingInjectedClient()
        {
            using var senderVariable = new TestEnvironmentVariable("books@example.com");
            Assert.Throws<ArgumentNullException>(() => new SendGridEmailSender(Settings(senderVariable.Name), null));
        }

        private static IOptions<SendGridSettings> Settings(string senderVariable) => Options.Create(new SendGridSettings
        {
            SenderNameFrom = "BookShop",
            SenderEmailFromKey = senderVariable,
            ApiKey = "BOOKSHOP_UNUSED_TEST_API_KEY"
        });

        private sealed class TestEnvironmentVariable : IDisposable
        {
            public string Name { get; } = "BOOKSHOP_TEST_" + Guid.NewGuid().ToString("N");

            public TestEnvironmentVariable(string value) => Environment.SetEnvironmentVariable(Name, value);

            public void Dispose() => Environment.SetEnvironmentVariable(Name, null);
        }
    }
}
