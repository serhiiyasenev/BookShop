using BusinessLayer.Interfaces;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
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
        /// Creates a Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns>A newly created Product item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///     "name": "The Three Musketeers",
        ///     "description": "You have likely heard of The Three Musketeers! This story has been reproduced into films, TV series, and other novels..." ,
        ///     "author": "Alexandre Dumas",
        ///     "price": 12.50,
        ///     "imageUrl": "ftp://book.shop/downloads/image.jpg"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(ProductOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> AddProduct(ProductInbound product)
        {
            var createdProduct = await _productService.AddItem(product);
            _logger.LogInformation($"Product was created with id: '{createdProduct.Id}'");
            return CreatedAtAction(nameof(AddProduct), createdProduct);
        }

        /// <summary>
        /// Get all Products endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns all Products from a storage
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IQueryable<ProductOutbound>))]
        public IActionResult GetAllProducts()
        {
            return Ok(_productService.GetAllItems());
        }

        /// <summary>
        /// Get Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns pointed by it's Guid Product from a storage
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(ProductOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetItemById(id);
            return product != null ? Ok(product) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }

        /// <summary>
        /// Update Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns newly updated Product by Guid
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(ProductOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateProductById(Guid id, ProductInbound product)
        {
            var updatedProduct = await _productService.UpdateItemById(id, product);
            return updatedProduct != null ? Ok(updatedProduct) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }

        /// <summary>
        /// Delete Product by id endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns pointed Guid
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(SimpleResult))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.RemoveItemById(id);
            return result > 0 ? Ok(new SimpleResult { Result = $"Product with id '{id}' was deleted" }) 
                              : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }
    }
}
