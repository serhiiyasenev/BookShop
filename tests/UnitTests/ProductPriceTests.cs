using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using AutoMapper;
using BusinessLayer.Mappings;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer;
using DataAccessLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
