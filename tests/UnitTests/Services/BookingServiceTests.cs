using System;
using AutoMapper;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Services;
using DataAccessLayer.Interfaces;
using Moq;
using NUnit.Framework;

namespace UnitTests.Services
{
    public class BookingServiceTests
    {
        [Test]
        public void Add_WithoutProducts_RejectsBookingBeforeAccessingStorage()
        {
            var bookings = new Mock<IBookingRepository>(MockBehavior.Strict);
            var products = new Mock<IProductRepository>(MockBehavior.Strict);
            var mapper = new Mock<IMapper>(MockBehavior.Strict);
            var service = new BookingService(mapper.Object, bookings.Object, products.Object);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.AddItem(new BookingInbound { Products = Array.Empty<Guid>() }));

            bookings.VerifyNoOtherCalls();
            products.VerifyNoOtherCalls();
            mapper.VerifyNoOtherCalls();
        }
    }
}
