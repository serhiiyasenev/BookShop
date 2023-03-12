using Api.Controllers;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebUI.Controllers;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class ProductsUIControllerTests : BaseTest
    {
        private Mock<IProductService> _productServiceMock;
        private Mock<IOptions<AllowedExtensions>> _allowedExtensionsMock;
        private Mock<IFileUploadService> _fileUploadServiceMock;
        private ProductsController _productController;
        private DefaultHttpContext _httpContext;
        private TempDataDictionary _tempData;

        [SetUp]
        public void SetUp()
        {
            _productServiceMock = new Mock<IProductService>();
            _fileUploadServiceMock = new Mock<IFileUploadService>();
            _allowedExtensionsMock = new Mock<IOptions<AllowedExtensions>>();
            _allowedExtensionsMock.SetupGet(x => x.Value)
                                  .Returns(new AllowedExtensions { ImageAllowed = ".jpg;.png;.jpeg" });
            _httpContext = new DefaultHttpContext();
            _tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
            _productController = new ProductsController(_productServiceMock.Object, _allowedExtensionsMock.Object, _fileUploadServiceMock.Object)
            {
                TempData = _tempData
            };
        }

        [Test]
        public async Task CreateProduct_Saves_NewlyCreatedProduct()
        {
            // Arrange
            var expectedProductOutbound = new ProductOutbound();
            _productServiceMock.Setup(x => x.AddItem(It.IsAny<ProductInbound>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedProductOutbound);

            // Act
            var result = await _productController.Create(It.IsAny<ProductInbound>());

            // Assert
            result.Should().NotBeNull();
            _productController.TempData["Created"].Should().Be("New product created!");
            _productServiceMock.Verify(x => x.AddItem(It.IsAny<ProductInbound>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
