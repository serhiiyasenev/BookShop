using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Mappings;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Models;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.Mapper
{
    [TestFixture]
    public class BookingProfileTests : BaseTest
    {
        private IMapper _mapper;

        public BookingProfileTests()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<BookingProfile>());
            _mapper = new AutoMapper.Mapper(configuration);
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void ShouldMapRequestModelToItemsRequest()
        {
            // Arrange
            var requestModel = new RequestModel { Name = "Test", Page = 1, PageSize = 10 };

            // Act
            var result = _mapper.Map<ItemsRequest>(requestModel);

            // Assert
            result.ItemName.Should().Be(requestModel.Name);
            result.PageNumber.Should().Be(requestModel.Page);
            result.PageSize.Should().Be(requestModel.PageSize);
        }

        [Test]
        public void ShouldMapProductInboundToProductDto()
        {
            // Arrange
            var productInbound = new ProductInbound
            {
                Name = "Test",
                Description = "Test Description",
                Author = "Test Author",
                Price = 10.12f,
                ImageUrl = "http://example.com/image.jpg"
            };

            // Act
            var result = _mapper.Map<ProductDto>(productInbound);

            // Assert
            result.Id.Should().Be(Guid.Empty);
            result.Name.Should().Be(productInbound.Name);
            result.Description.Should().Be(productInbound.Description);
            result.Author.Should().Be(productInbound.Author);
            result.Price.Should().Be(productInbound.Price);
            result.ImageUrl.Should().Be(productInbound.ImageUrl);
            result.BookingDtoId.Should().Be(null);
        }

        [Test]
        public void ShouldMapProductDtoToProductOutbound()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Description = "Test Description",
                Author = "Test Author",
                Price = 10.01f,
                ImageUrl = "http://example.com/image.jpg",
                BookingDtoId = Guid.NewGuid()
            };

            var productDto2 = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Test 2",
                Description = "Test Description 2",
                Author = "Test Author 2",
                Price = 10.22f,
                ImageUrl = "http://example.com/image2.jpg",
                BookingDtoId = null
            };

            // Act
            var result = _mapper.Map<ProductOutbound>(productDto);
            var result2 = _mapper.Map<ProductOutbound>(productDto2);

            // Assert
            result.Should().BeEquivalentTo(productDto, opt => opt.Excluding(d => d.BookingDtoId));
            result.BookingId.Should().Be(productDto.BookingDtoId);

            result2.Should().BeEquivalentTo(productDto2, opt => opt.Excluding(d => d.BookingDtoId));
            result2.BookingId.Should().Be(productDto2.BookingDtoId);
        }

        [Test]
        public void ShouldMapBookingInboundToBookingDto()
        {
            // Arrange
            var bookingInbound = new BookingInbound
            {
                Name = "Test",
                CustomerEmail = "test@example.com",
                DeliveryAddress = "Test Address",
                DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Products = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
            };

            // Act
            var bookingDto = _mapper.Map<BookingDto>(bookingInbound);

            // Assert
            bookingDto.Should().BeEquivalentTo(bookingInbound, opt 
                => opt.Excluding(d => d.Status).Excluding(d => d.CreatedDate).Excluding(d => d.Products));

            bookingDto.CreatedDate.Should().BeCloseTo(bookingInbound.CreatedDate, TimeSpan.FromSeconds(1));
            bookingDto.Status.Should().Be((int)bookingInbound.Status);
            CollectionAssert.AreEqual(bookingInbound.Products, bookingDto.Products.Select(p => p.Id));
        }

        [Test]
        public void ShouldMapBookingDtoToBookingOutbound()
        {
            // Arrange
            var dt = DateTime.UtcNow;
            var createdDateDto = dt.AddTicks(-(dt.Ticks % TimeSpan.TicksPerSecond));
            var testGuid = Guid.NewGuid();
            var deliveryDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var bookingDto = new BookingDto
            {
                Id = testGuid,
                Name = "Test",
                CustomerEmail = "test@example.com",
                DeliveryAddress = "Test Address",
                DeliveryDate = deliveryDate,
                CreatedDate = createdDateDto,
                Status = 0,
                Products = new List<ProductDto>
                {
                    new ProductDto
                    {
                        Id = testGuid,
                        Name = "Product 1",
                        Description = "Description 1",
                        Author = "Author 1",
                        Price = 10.0f,
                        ImageUrl = "http://example.com/image1.jpg",
                        BookingDtoId = testGuid
                    },
                    new ProductDto
                    {
                        Id = testGuid,
                        Name = "Product 2",
                        Description = "Description 2",
                        Author = "Author 2",
                        Price = 15.0f,
                        ImageUrl = "http://example.com/image2.jpg",
                        BookingDtoId = testGuid
                    }
                }
            };

            var expectedBookingOutbound = new BookingOutbound
            {
                Id = testGuid,
                Name = "Test",
                CustomerEmail = "test@example.com",
                DeliveryAddress = "Test Address",
                DeliveryDate = deliveryDate,
                CreatedDate = createdDateDto,
                Status = BookingStatus.Submitted,
                Products = new List<ProductOutbound>
                {
                    new ProductOutbound
                    {
                        Id = testGuid,
                        Name = "Product 1",
                        Description = "Description 1",
                        Author = "Author 1",
                        Price = 10.0f,
                        ImageUrl = "http://example.com/image1.jpg",
                        BookingId = testGuid
                    },
                    new ProductOutbound
                    {
                        Id = testGuid,
                        Name = "Product 2",
                        Description = "Description 2",
                        Author = "Author 2",
                        Price = 15.0f,
                        ImageUrl = "http://example.com/image2.jpg",
                        BookingId = testGuid
                    }
                }
            };

            // Act
            var bookingOutbound = _mapper.Map<BookingOutbound>(bookingDto);

            // Assert
            bookingOutbound.Should().BeEquivalentTo(expectedBookingOutbound);
        }
    }
}
