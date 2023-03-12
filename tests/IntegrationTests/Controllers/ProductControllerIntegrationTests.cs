using Api;
using Api.Helpers;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntegrationTests.Controllers
{
    [TestFixture]
    public class ProductControllerIntegrationTests
    {
        private readonly string _requestUri;
        private readonly HttpClient _httpClient;
        private readonly WebApplicationFactory<Startup> _factory;

        public ProductControllerIntegrationTests()
        {
            _requestUri = "/product";
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => 
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(EfCoreContext));
                        services.RemoveAll(typeof(DbContextOptions<EfCoreContext>));
                        services.AddDbContext<EfCoreContext>(options => options.UseInMemoryDatabase("TestDb"));
                    }
            ));
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

        [Test]
        public async Task GetAllProducts_Returns_ResponseModelWithProduct()
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
            var allProduct = await (await _httpClient.GetAsync(_requestUri))
                .EnsureSuccessStatusCode().GetModelAsync<ResponseModel<ProductOutbound>>();
            var createdProductResult = allProduct.Items.Single(e => e.Id.Equals(createdProduct.Id));

            // Assert
            allProduct.Should().BeOfType<ResponseModel<ProductOutbound>>();
            allProduct.Items.ToList().Should().HaveCountGreaterThanOrEqualTo(1);
            createdProductResult.Id.Should().Be(createdProduct.Id);

            Assert.AreEqual(productInbound.Name, createdProductResult.Name);
            Assert.AreEqual(productInbound.Description, createdProductResult.Description);
            Assert.AreEqual(productInbound.Author, createdProductResult.Author);
            Assert.AreEqual(productInbound.Price, createdProductResult.Price);
            Assert.AreEqual(productInbound.ImageUrl, createdProductResult.ImageUrl);
        }

        [Test]
        public async Task GetProductbyId_Returns_CreatedProduct()
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
            var createdProductById = await (await _httpClient.GetAsync($"{_requestUri}/{createdProduct.Id}"))
                 .EnsureSuccessStatusCode().GetModelAsync<ProductOutbound>();

            // Assert
            createdProductById.Should().BeOfType<ProductOutbound>();
            createdProductById.Should().BeEquivalentTo(createdProduct);
        }

        [Test]
        public async Task DeleteProductbyId_Returns_Message()
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
            var deletedProduct = await (await _httpClient.DeleteAsync($"{_requestUri}/{createdProduct.Id}"))
                 .EnsureSuccessStatusCode().GetModelAsync<SimpleResult>();

            // Assert
            deletedProduct.Should().BeOfType<SimpleResult>();
            deletedProduct.Result.Should().Be($"Product with id '{createdProduct.Id}' was deleted");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _httpClient.Dispose();
            await _factory.DisposeAsync();
        }
    }
}
