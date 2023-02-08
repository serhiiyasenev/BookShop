using Api.Helpers;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Inbound.Product;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly ImageStorageSettings _settings;
        private readonly HttpContext _httpContext;
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService<ProductInbound, ProductOutbound> _productService;

        public ProductController(ILogger<ProductController> logger, IOptions<ImageStorageSettings> settings,
            IProductService<ProductInbound, ProductOutbound> productService, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _settings = settings.Value;
            _productService = productService;
            _httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Create Product
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
        /// Upload image file to local or cloud storage
        /// </summary>
        /// <param name="image"></param>
        /// <response code="200">Returns successfully saved message</response>
        /// <response code="400">If the item is incorrect</response>
        /// <response code="500">If internal server error</response>
        [HttpPost]
        [Route("Image")]
        [ProducesResponseType(200, Type = typeof(SimpleResult))]
        [ProducesResponseType(400, Type = typeof(SimpleResult))]
        [ProducesResponseType(500, Type = typeof(SimpleResult))]
        public async Task<IActionResult> AddProductImage(IFormFile image)
        {
            string fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant().Replace(".", "");
            if (!_settings.AllowedExtensions.Split(";").ToList().Contains(fileExtension))
            {
                return BadRequest(new SimpleResult { Result = $"Not Allowed Extension `{fileExtension}`, extension should be from `{_settings.AllowedExtensions}`" });
            }

            var imagePath = Path.Combine(_settings.StoragePath, image.FileName);
            var (saved, message) = await _productService.SaveImage(imagePath, image.OpenReadStream());
            if (saved)
            {
                _logger.LogInformation($"Image `{image.FileName}` saved to Image Storage `{_settings.StoragePath}`'");
                return Ok(new SimpleResult { Result = $"Image `{image.FileName}` successfully saved to Image Storage" });
            }
            else
            {
                _logger.LogInformation($"Image `{image.FileName}` cannot be saved to Image Storage `{_settings.StoragePath}` due to `{message}`'");
                return StatusCode(500, new SimpleResult { Result = $"Image `{image.FileName}`cannot be saved to Image Storage now. {message}" });
            }
        }

        /// <summary>
        /// Get all Products endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns all Products from a storage
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ResponseModel<ProductOutbound>))]
        public ActionResult<ResponseModel<ProductOutbound>> GetAllProducts([FromQuery] GetItemsRequest request)
        {
            var requestTest = request;
            var contextTest = _httpContext;
            // it will be updated to get results via predicates From Query string and Context
            var products = _productService.GetAllItems();
            var result = new ResponseModel<ProductOutbound>
            {
                Items = products,
                TotalCount = products.Count()
            };
            return Ok(result);
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
