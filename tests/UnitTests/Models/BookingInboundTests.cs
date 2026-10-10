using System;
using System.Globalization;
using BusinessLayer.Models.Inbound;
using NUnit.Framework;

namespace UnitTests.Models
{
    public class BookingInboundTests
    {
        [TestCase("en-US")]
        [TestCase("uk-UA")]
        public void DeliveryDate_PreservesTheSameCalendarDateAcrossCultures(string culture)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                var expected = new DateOnly(DateTime.UtcNow.Year + 1, 12, 31);
                var booking = new BookingInbound { DeliveryDate = expected };

                Assert.That(booking.DeliveryDate, Is.EqualTo(expected));
            }
            finally { CultureInfo.CurrentCulture = originalCulture; }
        }

        [Test]
        public void DeliveryDate_RejectsPastDateWithoutChangingAcceptedValue()
        {
            var accepted = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
            var booking = new BookingInbound { DeliveryDate = accepted };

            Assert.Throws<ArgumentException>(() => booking.DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

            Assert.That(booking.DeliveryDate, Is.EqualTo(accepted));
        }
    }
}
