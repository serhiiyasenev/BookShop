using Api.Controllers;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ProductControllerTests
    {
        private Mock<ILogger<ProductController>> _logger;
        private Mock<IProductService> _productServiceMock;
        private Mock<ILogger<ProductController>> _loggerMock;
        private Mock<IOptions<AllowedExtensions>> _allowedExtensionsMock;
        private Mock<IFileUploadService> _fileUploadServiceMock;
        private ProductController _productController;

        [SetUp]
        public void SetUp()
        {
            _logger= new Mock<ILogger<ProductController>>();
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductController>>();
            _allowedExtensionsMock = new Mock<IOptions<AllowedExtensions>>();
            _fileUploadServiceMock = new Mock<IFileUploadService>();
            _productController = new ProductController(_loggerMock.Object, _productServiceMock.Object, 
                                               _allowedExtensionsMock.Object, _fileUploadServiceMock.Object);
        }

        [Test]
        public async Task AddProduct_Returns_CorrectModel()
        {
            // Arrange
            var productInbound = new ProductInbound
            {
                Name = $"The Three Musketeers {DateTime.UtcNow.Ticks}",
                Description = $"You have likely heard of The Three Musketeers! {DateTime.UtcNow.Ticks}",
                Author = $"Alexandre Dumas {DateTime.UtcNow.Ticks}",
                Price = 12.56f,
                ImageUrl = $"ftp://book.shop/{DateTime.UtcNow.Ticks}/image.jpg"
            };

            var expectedOutbound = new ProductOutbound
            {
                Id = Guid.NewGuid(),
                Name = productInbound.Name,
                Description = productInbound.Description,
                Author = productInbound.Author,
                Price = productInbound.Price,
                ImageUrl = productInbound.ImageUrl,
                BookingId = null
            };

            _productServiceMock.Setup(x => x.AddItem(productInbound, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(expectedOutbound);

            // Act
            var result = await _productController.AddProduct(productInbound);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsAssignableFrom<CreatedAtActionResult>(result);
            Assert.IsNotNull(createdResult);

            var actualResult = createdResult.Value as ProductOutbound;
            Assert.IsInstanceOf<ProductOutbound>(actualResult);
            _productServiceMock.Verify(x => x.AddItem(productInbound, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AddProduct_Returns_CorrectProduct()
        {
            // Arrange
            var productInbound = new ProductInbound
            {
                Name = $"The Three Musketeers {DateTime.UtcNow.Ticks}",
                Description = $"You have likely heard of The Three Musketeers! {DateTime.UtcNow.Ticks}",
                Author = $"Alexandre Dumas {DateTime.UtcNow.Ticks}",
                Price = 12.56f,
                ImageUrl = $"ftp://book.shop/{DateTime.UtcNow.Ticks}/image.jpg"
            };

            var expectedOutbound = new ProductOutbound
            {
                Id = Guid.NewGuid(),
                Name = productInbound.Name,
                Description = productInbound.Description,
                Author = productInbound.Author,
                Price = productInbound.Price,
                ImageUrl = productInbound.ImageUrl,
                BookingId = null
            };

            _productServiceMock.Setup(x => x.AddItem(productInbound, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(expectedOutbound);

            // Act
            var result = await _productController.AddProduct(productInbound);
            var actualResult = ((CreatedAtActionResult)result).Value as ProductOutbound;

            // Assert
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.Id, Is.EqualTo(expectedOutbound.Id));
            Assert.That(actualResult.Name, Is.EqualTo(expectedOutbound.Name));
            Assert.That(actualResult.Description, Is.EqualTo(expectedOutbound.Description));
            Assert.That(actualResult.Author, Is.EqualTo(expectedOutbound.Author));
            Assert.That(actualResult.Price, Is.EqualTo(expectedOutbound.Price));
            Assert.That(actualResult.ImageUrl, Is.EqualTo(expectedOutbound.ImageUrl));
            Assert.That(actualResult.BookingId, Is.EqualTo(expectedOutbound.BookingId));
        }
    }
}