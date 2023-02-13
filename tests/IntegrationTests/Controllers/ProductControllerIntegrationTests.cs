using Api.Helpers;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using Microsoft.eShopWeb.FunctionalTests.Web.Controllers;
using NUnit.Framework;

namespace IntegrationTests.Controllers
{
    [TestFixture]
    public class ProductControllerIntegrationTests
    {
        private readonly string _requestUri;
        private readonly HttpClient _httpClient;
        private readonly CustomWebApplicationFactory _factory;

        public ProductControllerIntegrationTests()
        {
            _requestUri = "/product";
            _factory = new CustomWebApplicationFactory();
            _httpClient = _factory.CreateClient();
        }

        [Test]
        public async Task PostProduct_Returns_NewlyCreatedProduct()
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

            var content = JsonHelper.ToStringContent(productInbound);


            // Act
            var createdProduct = await (await _httpClient.PostAsync(_requestUri, content))
                 .EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();

            // Assert
            Assert.IsNotNull(createdProduct.Id);
            Assert.IsNull(createdProduct.BookingId);

            Assert.AreEqual(productInbound.Name, createdProduct.Name);
            Assert.AreEqual(productInbound.Description, createdProduct.Description);
            Assert.AreEqual(productInbound.Author, createdProduct.Author);
            Assert.AreEqual(productInbound.Price, createdProduct.Price);
            Assert.AreEqual(productInbound.ImageUrl, createdProduct.ImageUrl);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _httpClient.Dispose();
            await _factory.DisposeAsync();
        }
    }
}
