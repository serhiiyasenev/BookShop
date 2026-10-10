using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Mappings;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Services;
using DataAccessLayer;
using DataAccessLayer.DTO;
using DataAccessLayer.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IntegrationTests
{
    public class BookingPersistenceTests
    {
        [Test]
        public async Task Booking_CreateAppendAndChangeStatus_PersistExistingProductsInSqlServer()
        {
            var connectionString = Environment.GetEnvironmentVariable("BOOKSHOP_TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Ignore("Set BOOKSHOP_TEST_SQLSERVER to run the real SQL Server booking test.");
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "BookShopBookingTests_" + Guid.NewGuid().ToString("N")
            };
            var options = new DbContextOptionsBuilder<EfCoreContext>()
                .UseSqlServer(builder.ConnectionString, sql => sql.EnableRetryOnFailure()).Options;
            using var setup = new EfCoreContext(options);
            try
            {
                await setup.Database.MigrateAsync();
                var original = new ProductDto { Id = Guid.NewGuid(), Name = "Original book", Price = 19.49m };
                var additional = new ProductDto { Id = Guid.NewGuid(), Name = "Additional book", Price = 12.34m };
                setup.Products.AddRange(original, additional);
                await setup.SaveChangesAsync();
                var mapper = new MapperConfiguration(cfg => cfg.AddProfile<BookingProfile>()).CreateMapper();
                Guid bookingId;
                DateTime createdDate;

                using (var db = new EfCoreContext(options))
                {
                    var service = Service(db, mapper);
                    var created = await service.AddItem(Request(original.Id));
                    bookingId = created.Id;
                    createdDate = created.CreatedDate;
                }
                using (var db = new EfCoreContext(options))
                {
                    var stored = await db.Products.AsNoTracking().SingleAsync(p => p.Id == original.Id);
                    Assert.That(stored.BookingDtoId, Is.EqualTo(bookingId));
                    Assert.That(await db.Products.CountAsync(), Is.EqualTo(2), "Existing products must not be inserted again.");
                }
                using (var db = new EfCoreContext(options))
                {
                    var update = Request(additional.Id);
                    update.Name = "Updated SQL booking";
                    await Service(db, mapper).UpdateItemById(bookingId, update);
                }
                using (var db = new EfCoreContext(options))
                    await Service(db, mapper).UpdateItemStatusById(bookingId, BookingStatus.InDelivery);

                using (var db = new EfCoreContext(options))
                {
                    var persisted = await db.Bookings.AsNoTracking().Include(b => b.Products).SingleAsync();
                    Assert.That(persisted.Name, Is.EqualTo("Updated SQL booking"));
                    Assert.That(persisted.CreatedDate, Is.EqualTo(createdDate));
                    Assert.That(persisted.Status, Is.EqualTo((int)BookingStatus.InDelivery));
                    Assert.That(persisted.Products.Select(p => p.Id), Is.EquivalentTo(new[] { original.Id, additional.Id }));
                    Assert.That(persisted.Products.All(p => p.BookingDtoId == bookingId), Is.True);
                    Assert.That(persisted.Products.Single(p => p.Id == original.Id).Price, Is.EqualTo(19.49m));
                    Assert.That(persisted.Products.Single(p => p.Id == additional.Id).Price, Is.EqualTo(12.34m));
                    Assert.That(await db.Products.CountAsync(), Is.EqualTo(2));
                }
            }
            finally { await setup.Database.EnsureDeletedAsync(); }
        }

        private static BookingService Service(EfCoreContext db, IMapper mapper) =>
            new(mapper, new BookingDbRepository(db), new ProductDbRepository(db));

        private static BookingInbound Request(params Guid[] products) => new()
        {
            Name = "SQL booking test", CustomerEmail = "reader@example.com", DeliveryAddress = "20 Book Street, Kyiv",
            DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), Products = products
        };
    }
}
