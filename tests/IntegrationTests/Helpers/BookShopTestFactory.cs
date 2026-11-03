using System;
using System.Collections.Generic;
using System.Net.Http;
using Api.Security;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using DataAccessLayer;
using InfrastructureLayer.Email.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Helpers
{
    internal sealed class BookShopTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        internal static readonly string TestAdminKey = new string('T', 40);
        private readonly bool _configureAdminKey;
        private readonly string _databaseName = "BookShopScenario_" + Guid.NewGuid().ToString("N");

        public BookShopTestFactory(bool configureAdminKey = true)
        {
            _configureAdminKey = configureAdminKey;
        }

        public HttpClient CreateAuthorizedClient()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add(AdminApiKeyAuthorizationFilter.HeaderName, TestAdminKey);
            return client;
        }
        public RecordingEmailSender Email { get; } = new RecordingEmailSender();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Security:AdminApiKey"] = _configureAdminKey ? TestAdminKey : ""
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<EfCoreContext>();
                services.RemoveAll<DbContextOptions<EfCoreContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<EfCoreContext>>();
                services.AddDbContext<EfCoreContext>(options => options.UseInMemoryDatabase(_databaseName));
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(Email);
            });
        }

        public async Task SeedAsync(params object[] entities)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfCoreContext>();
            db.AddRange(entities);
            await db.SaveChangesAsync();
        }

        public async Task<T> QueryAsync<T>(Func<EfCoreContext, Task<T>> query)
        {
            using var scope = Services.CreateScope();
            return await query(scope.ServiceProvider.GetRequiredService<EfCoreContext>());
        }
    }

    internal sealed class RecordingEmailSender : IEmailSender
    {
        public List<(string Recipient, string Subject, string Body)> Messages { get; } = new();

        public Task<(bool, string)> SendEmailAsync(string emailTo, string subject, string message)
        {
            Messages.Add((emailTo, subject, message));
            return Task.FromResult((true, "Recorded by the test; no external email sent."));
        }
    }
}
