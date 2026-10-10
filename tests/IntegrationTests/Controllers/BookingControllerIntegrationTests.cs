using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BusinessLayer.Enums;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IntegrationTests.Controllers
{
    public class BookingControllerIntegrationTests
    {
        private BookShopTestFactory<Api.Startup> _factory;
        private HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        [SetUp]
        public void SetUp()
        {
            _factory = new BookShopTestFactory<Api.Startup>();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Create_LinksExistingProductAndSendsBookingDetails()
        {
            var product = Product("Vacation book");
            await _factory.SeedAsync(product);
            var request = Booking(product.Id);
            using var response = await _client.PostAsJsonAsync("/Booking", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await response.Content.ReadAsStringAsync());
            var created = await response.Content.ReadFromJsonAsync<BookingOutbound>(JsonOptions);
            var persisted = await _client.GetFromJsonAsync<BookingOutbound>($"/Booking/{created.Id}", JsonOptions);
            Assert.That(persisted.Name, Is.EqualTo(request.Name));
            Assert.That(persisted.Status, Is.EqualTo(BookingStatus.Submitted));
            Assert.That(persisted.DeliveryDate, Is.EqualTo(request.DeliveryDate));
            Assert.That(persisted.Products.Single().Id, Is.EqualTo(product.Id));
            Assert.That(persisted.Products.Single().Price, Is.EqualTo(19.49m));
            Assert.That(await _factory.QueryAsync(db => db.Products.CountAsync()), Is.EqualTo(1), "Booking must reuse the catalog row.");
            var storedProduct = await _factory.QueryAsync(db => db.Products.AsNoTracking().SingleAsync());
            Assert.That(storedProduct.BookingDtoId, Is.EqualTo(created.Id));
            var email = _factory.Email.Messages.Single();
            Assert.That(email.Recipient, Is.EqualTo(request.CustomerEmail));
            Assert.That(email.Subject, Is.EqualTo("Your booking was created"));
            Assert.That(email.Body, Does.Contain(product.Name).And.Contain(request.DeliveryAddress).And.Contain("Submitted"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Update_PreservesOriginalProductsAndPersistsNewLinks(bool addProduct)
        {
            var existing = StoredBooking("Original booking");
            var extra = Product("Additional book");
            await _factory.SeedAsync(existing, extra);
            var update = Booking(addProduct ? new[] { extra.Id } : Array.Empty<Guid>());
            update.Name = "Updated booking";
            update.CustomerEmail = "updated@example.com";
            update.DeliveryAddress = "Updated delivery address";

            using var response = await _client.PutAsJsonAsync($"/Booking/{existing.Id}", update);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
            var persisted = await _client.GetFromJsonAsync<BookingOutbound>($"/Booking/{existing.Id}", JsonOptions);

            Assert.That(persisted.Name, Is.EqualTo(update.Name));
            Assert.That(persisted.CustomerEmail, Is.EqualTo(update.CustomerEmail));
            Assert.That(persisted.DeliveryAddress, Is.EqualTo(update.DeliveryAddress));
            Assert.That(persisted.CreatedDate, Is.EqualTo(existing.CreatedDate));
            Assert.That(persisted.Status, Is.EqualTo(BookingStatus.Approved));
            var expectedIds = addProduct ? new[] { existing.Products.Single().Id, extra.Id } : new[] { existing.Products.Single().Id };
            Assert.That(persisted.Products.Select(p => p.Id), Is.EquivalentTo(expectedIds));
            var extraStored = await _factory.QueryAsync(db => db.Products.AsNoTracking().SingleAsync(p => p.Id == extra.Id));
            Assert.That(extraStored.BookingDtoId, Is.EqualTo(addProduct ? (Guid?)existing.Id : null));
            Assert.That(extraStored.Price, Is.EqualTo(extra.Price));
            Assert.That(extraStored.Name, Is.EqualTo(extra.Name));
        }

        [Test]
        public async Task UpdateStatus_PersistsStatusAndEmailsCustomer()
        {
            var existing = StoredBooking("Delivery booking");
            await _factory.SeedAsync(existing);

            using var request = new HttpRequestMessage(HttpMethod.Patch, $"/Booking/{existing.Id}?bookingStatus=InDelivery");
            using var response = await _client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("InDelivery"));
            var persisted = await _client.GetFromJsonAsync<BookingOutbound>($"/Booking/{existing.Id}", JsonOptions);
            Assert.That(persisted.Status, Is.EqualTo(BookingStatus.InDelivery));
            var email = _factory.Email.Messages.Single();
            Assert.That(email.Recipient, Is.EqualTo(existing.CustomerEmail));
            Assert.That(email.Subject, Is.EqualTo("Your booking status was updated"));
            Assert.That(email.Body, Does.Contain("InDelivery").And.Contain(existing.Products.Single().Name));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task List_FiltersBeforePaginationAndReturnsTotalCount(bool filter)
        {
            await _factory.SeedAsync(StoredBooking("Holiday one"), StoredBooking("Holiday two"), StoredBooking("Work reading"));
            var path = "/Booking?Page=2&PageSize=1" + (filter ? "&Name=Holiday" : "");

            var page = await _client.GetFromJsonAsync<ResponseModel<BookingOutbound>>(path, JsonOptions);

            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(1));
            Assert.That(page.TotalCount, Is.EqualTo(filter ? 2 : 3));
            Assert.That(page.Items.Count(), Is.EqualTo(1));
            Assert.That(page.Items.Single().Products.Count(), Is.EqualTo(1));
            if (filter) Assert.That(page.Items.Single().Name, Does.Contain("Holiday"));
        }

        [TestCase("GET")]
        [TestCase("PUT")]
        [TestCase("PATCH")]
        public async Task MissingBooking_ReturnsNotFoundWithoutEmail(string method)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), $"/Booking/{Guid.NewGuid()}?bookingStatus=Approved");
            if (method == "PUT") request.Content = JsonContent.Create(Booking());
            using var response = await _client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(_factory.Email.Messages, Is.Empty);
            Assert.That(await _factory.QueryAsync(db => db.Bookings.CountAsync()), Is.Zero);
        }

        [Test]
        public async Task Create_WithMissingProduct_ReturnsNotFoundWithoutWritingOrEmailing()
        {
            var missingId = Guid.NewGuid();
            using var response = await _client.PostAsJsonAsync("/Booking", Booking(missingId));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.That(problem.Detail, Does.Contain(missingId.ToString()));
            Assert.That(await _factory.QueryAsync(db => db.Bookings.CountAsync()), Is.Zero);
            Assert.That(_factory.Email.Messages, Is.Empty);
        }

        [Test]
        public async Task Create_WithAlreadyBookedProduct_ReturnsConflictAndKeepsOriginalBooking()
        {
            var existing = StoredBooking("Existing booking");
            await _factory.SeedAsync(existing);
            using var response = await _client.PostAsJsonAsync("/Booking", Booking(existing.Products.Single().Id));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.That(problem.Title, Is.EqualTo("Product already linked"));
            Assert.That(await _factory.QueryAsync(db => db.Bookings.CountAsync()), Is.EqualTo(1));
            var product = await _factory.QueryAsync(db => db.Products.AsNoTracking().SingleAsync());
            Assert.That(product.BookingDtoId, Is.EqualTo(existing.Id));
            Assert.That(_factory.Email.Messages, Is.Empty);
        }

        [Test]
        public async Task Create_WithInvalidEmail_ReturnsValidationErrorWithoutWriting()
        {
            var booking = Booking(Guid.NewGuid());
            booking.CustomerEmail = "not-an-email";
            using var response = await _client.PostAsJsonAsync("/Booking", booking);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.That(problem.Errors.ContainsKey(nameof(BookingInbound.CustomerEmail)), Is.True);
            Assert.That(await _factory.QueryAsync(db => db.Bookings.CountAsync()), Is.Zero);
            Assert.That(_factory.Email.Messages, Is.Empty);
        }

        private static BookingInbound Booking(params Guid[] products) => new()
        {
            Name = "Holiday reading",
            CustomerEmail = "reader@example.com",
            DeliveryAddress = "20 Book Street, Kyiv",
            DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Products = products
        };

        private static ProductDto Product(string name) => new()
        {
            Id = Guid.NewGuid(), Name = name, Author = "Test Author", Price = 19.49m
        };

        private static BookingDto StoredBooking(string name) => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            CustomerEmail = "reader@example.com",
            DeliveryAddress = "20 Book Street, Kyiv",
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = (int)BookingStatus.Approved,
            Products = new[] { Product(name + " book") }
        };
    }
}
