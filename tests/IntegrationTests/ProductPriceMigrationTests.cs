using System;
using System.Threading.Tasks;
using DataAccessLayer;
using DataAccessLayer.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

namespace IntegrationTests
{
    public class ProductPriceMigrationTests
    {
        [Test]
        public async Task Migration_ConvertsLegacyPrice_AndPreservesNewDecimalAmounts()
        {
            var connectionString = Environment.GetEnvironmentVariable("BOOKSHOP_TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Ignore("Set BOOKSHOP_TEST_SQLSERVER to run the real SQL Server migration test.");

            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "BookShopPriceTests_" + Guid.NewGuid().ToString("N")
            };
            using var context = new EfCoreContext(new DbContextOptionsBuilder<EfCoreContext>()
                .UseSqlServer(builder.ConnectionString, sql => sql.EnableRetryOnFailure()).Options);
            try
            {
                var migrator = context.GetService<IMigrator>();
                await migrator.MigrateAsync("20230209204542_AddNameAndUpdateRestrictions");
                var id = Guid.NewGuid();
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO Products (Id, Name, Price) VALUES ({id}, {"Legacy book"}, {19.49f})");
                await migrator.MigrateAsync();

                Assert.That((await context.Products.SingleAsync(p => p.Id == id)).Price, Is.EqualTo(19.49m));
                context.Products.Add(new ProductDto { Id = Guid.NewGuid(), Name = "Exact price", Price = 1234567890.12m });
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                Assert.That((await context.Products.SingleAsync(p => p.Name == "Exact price")).Price, Is.EqualTo(1234567890.12m));
            }
            finally { await context.Database.EnsureDeletedAsync(); }
        }
    }
}
