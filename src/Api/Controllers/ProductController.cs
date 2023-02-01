using BusinessLayer.Interfaces;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core1WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService<ProductInbound, ProductOutbound> _productService;

        public ProductController(ILogger<ProductController> logger, IProductService<ProductInbound, ProductOutbound> productService)
        {
            _logger = logger;
            _productService = productService;
        }

        /// <summary>
        /// Creates an a new Product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     POST api/Product
        ///     {
        ///     "name": "The Three Musketeers",
        ///     "description": "You’ve likely heard of The Three Musketeers, or, Les Trois Mousquetaires even if you’re not a fan of French literature! This story has been reproduced into films, TV series, and other novels. This novel is considered to be the story that really put Dumas on the map.",
        ///     "author": "Alexandre Dumas",
        ///     "price": 12,
        ///     "imageUrl": "ftp://ftp.book.shop/downloads/image.jpg"
        ///     }
        /// </remarks>
        /// <param name="product"></param>
        /// <returns>The endpoint returns newly created Product with Guid</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>          
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddProduct(ProductInbound product)
        {
            if (product != null)
            {
                var createdProduct = await _productService.AddItem(product);
                _logger.LogInformation($"Product was created with id: '{createdProduct.Id}'");
                return Created(createdProduct.Id.ToString(), createdProduct);
            }

            return BadRequest("Product should not be null or empty");
        }

        /// <summary>
        /// Get all Products endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns all Products from a storage
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            return Ok(await _productService.GetAllItems());
        }

        /// <summary>
        /// Get Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns pointed by it's Guid Product from a storage
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetItemById(id);

            return product != null ? Ok(product) : NotFound($"NotFound by id: '{id}'");
        }

        /// <summary>
        /// Update Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns newly updated Product by Guid
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductById(Guid id, ProductInbound product)
        {
            var updatedProduct = await _productService.UpdateItemById(id, product);

            return updatedProduct != null ? Ok(updatedProduct) : NotFound($"NotFound by id: '{id}'");
        }

        /// <summary>
        /// Delete Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns pointed Guid
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.RemoveItemById(id);

            return result > 0 ? Ok($"Product with id '{id}' was deleted") : NotFound($"NotFound by id: '{id}'");
        }
    }
}
