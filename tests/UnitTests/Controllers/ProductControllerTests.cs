using Api.Controllers;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Text;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
            _logger = new Mock<ILogger<ProductController>>();
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductController>>();
            _fileUploadServiceMock = new Mock<IFileUploadService>();
            _allowedExtensionsMock = new Mock<IOptions<AllowedExtensions>>();
            _allowedExtensionsMock.SetupGet(x => x.Value).Returns(new AllowedExtensions { ImageAllowed = ".jpg;.png;.jpeg" });

            _productController = new ProductController(_loggerMock.Object, _productServiceMock.Object, 
                                               _allowedExtensionsMock.Object, _fileUploadServiceMock.Object);
        }

        [Test]
        public async Task AddProduct_Returns_CorrectModels()
        {
            // Arrange
            var expectedProductOutbound = new ProductOutbound();
            _productServiceMock.Setup(x => x.AddItem(It.IsAny<ProductInbound>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedProductOutbound);

            // Act
            var result = await _productController.AddProduct(It.IsAny<ProductInbound>());

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsAssignableFrom<CreatedAtActionResult>(result);
            Assert.IsNotNull(createdResult);

            var actualResult = createdResult.Value as ProductOutbound;
            Assert.IsInstanceOf<ProductOutbound>(actualResult);
            Assert.AreEqual(expectedProductOutbound, actualResult);
            _productServiceMock.Verify(x => x.AddItem(It.IsAny<ProductInbound>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AddProduct_Returns_CorrectProductValues()
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
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedOutbound.Id, actualResult.Id);
            Assert.AreEqual(expectedOutbound.Name, actualResult.Name);
            Assert.AreEqual(expectedOutbound.Description, actualResult.Description);
            Assert.AreEqual(expectedOutbound.Author, actualResult.Author);
            Assert.AreEqual(expectedOutbound.Price, actualResult.Price);
            Assert.AreEqual(expectedOutbound.ImageUrl, actualResult.ImageUrl);
            Assert.AreEqual(expectedOutbound.BookingId, actualResult.BookingId);
        }

        [Test]
        [TestCase(0, "NotFound by id: 'guid'")]
        [TestCase(1, "Product with id 'guid' was deleted")]
        public async Task DeleteProduct_Returns_SimpleResult(int result, string expectedMessage)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            _productServiceMock.Setup(x => x.RemoveItemById(id))
                .ReturnsAsync(result);
            expectedMessage = expectedMessage.Replace("guid", id.ToString());

            // Act
            var objectResult = await _productController.DeleteProduct(id);

            // Assert
            var responseBody = (SimpleResult)((ObjectResult)objectResult).Value;
            Assert.AreEqual(expectedMessage, responseBody?.Result);
        }

        [Test]
        [TestCase(true, "test-image.jpg", "Image `test-image.jpg` saved to Image Storage by path", 200)]
        [TestCase(false, "test-image.txt", "Not Allowed `test-image.txt`, extension should be from `.jpg;.png;.jpeg`", 400)]
        [TestCase(false, "test-image.jpg", "Failed to save image `test-image.jpg`to Image Storage now.", 500)]
        public async Task AddProductImage_Returns_SimpleResult(bool uploadResult, string fileName, string message, int expectedStatusCode)
        {
            // Arrange
            var imageMock = new Mock<IFormFile>(); 
            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test-image-content")); 
            imageMock.Setup(f => f.FileName).Returns(fileName);
            imageMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            var invockedCount = Times.Once;
            var expectedResult = (Result: uploadResult, Message: message);
            _fileUploadServiceMock.Setup(s => s.FileUpload(fileName, fileStream)).ReturnsAsync(expectedResult);

            // Act
            var result = await _productController.AddProductImage(imageMock.Object);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var statusCodeResult = (ObjectResult)result;
            var resultValue = (SimpleResult)((ObjectResult)result).Value;
            Assert.AreEqual(expectedStatusCode, statusCodeResult.StatusCode);
            Assert.AreEqual(expectedResult.Message, resultValue?.Result);

            if (expectedStatusCode == 400) invockedCount = Times.Never;

            _fileUploadServiceMock.Verify(x => x.FileUpload(fileName, fileStream), invockedCount);

            _loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(expectedResult.Message)),
                It.IsAny<Exception>(),It.Is<Func<It.IsAnyType, Exception?, string>>((f, e) => true)), invockedCount);
        }
    }
}