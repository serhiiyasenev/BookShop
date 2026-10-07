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
        [TestCase(-1.0)]
        [TestCase(1.0E20)]
        public async Task Migration_RejectsInvalidLegacyPrices_WithoutChangingSchema(double invalidPrice)
        {
            var connectionString = Environment.GetEnvironmentVariable("BOOKSHOP_TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Ignore("Set BOOKSHOP_TEST_SQLSERVER to run the real SQL Server migration test.");
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "BookShopInvalidPriceTests_" + Guid.NewGuid().ToString("N")
            };
            using var context = new EfCoreContext(new DbContextOptionsBuilder<EfCoreContext>()
                .UseSqlServer(builder.ConnectionString, sql => sql.EnableRetryOnFailure()).Options);
            try
            {
                var migrator = context.GetService<IMigrator>();
                await migrator.MigrateAsync("20230209204542_AddNameAndUpdateRestrictions");
                var id = Guid.NewGuid();
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO Products (Id, Name, Price) VALUES ({id}, {"Invalid legacy price"}, {invalidPrice})");
                var error = Assert.ThrowsAsync<SqlException>(async () => await migrator.MigrateAsync());
                Assert.That(error.Number, Is.EqualTo(50001));
                Assert.That(await context.Database.GetAppliedMigrationsAsync(),
                    Does.Not.Contain("20261007090000_UseDecimalProductPrices"));
                await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Products WHERE Id = {id}");
                await migrator.MigrateAsync();
            }
            finally { await context.Database.EnsureDeletedAsync(); }
        }

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
                await migrator.MigrateAsync("20230209204542_AddNameAndUpdateRestrictions");
                await context.Database.OpenConnectionAsync();
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Price'";
                Assert.That(await command.ExecuteScalarAsync(), Is.EqualTo("real"));
                await context.Database.CloseConnectionAsync();
                await migrator.MigrateAsync();
            }
            finally { await context.Database.EnsureDeletedAsync(); }
        }
    }
}
