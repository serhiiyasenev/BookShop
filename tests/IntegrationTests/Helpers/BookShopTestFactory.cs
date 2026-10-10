using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer;
using InfrastructureLayer.Email.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Helpers
{
    internal sealed class BookShopTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _databaseName = "BookShopScenario_" + Guid.NewGuid().ToString("N");
        public RecordingEmailSender Email { get; } = new RecordingEmailSender();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<EfCoreContext>();
                services.RemoveAll<DbContextOptions<EfCoreContext>>();
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
