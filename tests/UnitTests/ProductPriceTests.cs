using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using AutoMapper;
using BusinessLayer.Mappings;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using BusinessLayer.Services;
using DataAccessLayer;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    public class ProductPriceTests
    {
        [TestCase("0.10")]
        [TestCase("19.49")]
        [TestCase("1234567890.12")]
        [TestCase("9999999999999999.99")]
        public void Price_SurvivesJsonAndMappings_WithoutLosingCents(string text)
        {
            var inbound = JsonSerializer.Deserialize<ProductInbound>("{\"Price\":" + text + "}");
            var mapper = new MapperConfiguration(c => c.AddProfile<BookingProfile>()).CreateMapper();
            var entity = mapper.Map<ProductDto>(inbound);
            var outbound = mapper.Map<ProductOutbound>(entity);
            var roundTrip = JsonSerializer.Deserialize<ProductOutbound>(JsonSerializer.Serialize(outbound));

            Assert.That(roundTrip.Price, Is.EqualTo(decimal.Parse(text, CultureInfo.InvariantCulture)));
        }

        [TestCase("uk-UA")]
        [TestCase("en-US")]
        public void PriceValidation_EnforcesDatabaseRange_InEachCulture(string culture)
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (var model in new object[] { new ProductInbound(), new ProductDto(), new ProductOutbound() })
                {
                    var context = new ValidationContext(model) { MemberName = "Price" };
                    foreach (var value in new[] { 0m, 0.01m, 9999999999999999.99m })
                        Assert.That(Validator.TryValidateProperty(value, context, new List<ValidationResult>()), Is.True);
                    foreach (var value in new[] { -0.01m, 10000000000000000m })
                        Assert.That(Validator.TryValidateProperty(value, context, new List<ValidationResult>()), Is.False);
                }
            }
            finally { CultureInfo.CurrentCulture = original; }
        }

        [TestCase("uk-UA")]
        [TestCase("en-US")]
        public void PriceValidation_RejectsFractionalCents_AndAcceptsTrailingZeros(string culture)
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (var model in new object[] { new ProductInbound(), new ProductOutbound() })
                {
                    var context = new ValidationContext(model) { MemberName = "Price" };
                    foreach (var value in new[] { 0m, 0.0100m, 19.4900m, 1234567890.1200m, 9999999999999999.99m })
                        Assert.That(Validator.TryValidateProperty(value, context, new List<ValidationResult>()),
                            Is.True, $"{model.GetType().Name}: {value}");
                    foreach (var value in new[] { 0.001m, 19.499m, 19.999m, 0.0000000000000000000000000001m })
                    {
                        var errors = new List<ValidationResult>();
                        Assert.That(Validator.TryValidateProperty(value, context, errors),
                            Is.False, $"{model.GetType().Name}: {value}");
                        Assert.That(errors[0].ErrorMessage, Is.EqualTo("Price must have at most two decimal places."));
                    }
                }
            }
            finally { CultureInfo.CurrentCulture = original; }
        }

        [TestCase("0.001")]
        [TestCase("19.499")]
        [TestCase("-0.01")]
        [TestCase("10000000000000000")]
        public void Service_RejectsInvalidPrices_BeforeCallingRepository(string text)
        {
            var repository = new Mock<IProductRepository>(MockBehavior.Strict);
            var mapper = new MapperConfiguration(c => c.AddProfile<BookingProfile>()).CreateMapper();
            var service = new ProductService(mapper, repository.Object);
            var product = new ProductInbound { Name = "Price validation book", Price = decimal.Parse(text, CultureInfo.InvariantCulture) };

            Assert.ThrowsAsync<ValidationException>(() => service.AddItem(product));
            Assert.ThrowsAsync<ValidationException>(() => service.UpdateItemById(Guid.NewGuid(), product));
            repository.VerifyNoOtherCalls();
        }

        [Test]
        public void Price_UsesDecimalPrecisionInSqlServerModel()
        {
            using var context = new EfCoreContext(new DbContextOptionsBuilder<EfCoreContext>()
                .UseSqlServer("Server=localhost;Database=metadata-only;Integrated Security=true").Options);
            var price = context.Model.FindEntityType(typeof(ProductDto)).FindProperty(nameof(ProductDto.Price));
            Assert.That(price.ClrType, Is.EqualTo(typeof(decimal)));
            Assert.That(price.GetPrecision(), Is.EqualTo(18));
            Assert.That(price.GetScale(), Is.EqualTo(2));
            var snapshotPrice = context.GetService<IMigrationsAssembly>().ModelSnapshot.Model
                .FindEntityType(typeof(ProductDto).FullName).FindProperty(nameof(ProductDto.Price));
            Assert.That(snapshotPrice.ClrType, Is.EqualTo(price.ClrType));
            Assert.That(snapshotPrice.GetPrecision(), Is.EqualTo(price.GetPrecision()));
            Assert.That(snapshotPrice.GetScale(), Is.EqualTo(price.GetScale()));
        }
    }
}
