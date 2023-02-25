using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WebUI.Models.Products;

namespace WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly AllowedExtensions _allowedExtensions;
        private readonly IFileUploadService _fileUploadService;

        public ProductsController(IProductService productService, IOptions<AllowedExtensions> options, IFileUploadService fileUploadService)
        {
            _productService = productService;
            _allowedExtensions = options.Value;
            _fileUploadService = fileUploadService;
        }

        // GET: Products
        public async Task<IActionResult> Index(RequestModel requestModel)
        {
            var products = await _productService.GetAll(requestModel);
            var model = new IndexModel
            { 
                FilteredItems = products.FilteredItems,
                TotalCount = products.TotalCount,
                RequestModel = requestModel
            };
            return View(model);
        }

        // GET: Prodcuts/Details/{Guid}
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetItemById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Prodcuts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Prodcuts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInbound productInbound)
        {
            if (!ModelState.IsValid)
            {
                return View(productInbound);
            }
            var created = await _productService.AddItem(productInbound);
            TempData["Created"] = "New product created!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/{Guid}
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetItemById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/{Guid}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductOutbound productToUpdate)
        {
            var product = await _productService.GetItemById(id);
            if (product == null) return NotFound();
            if (!ModelState.IsValid) return View(productToUpdate);

            var productInbound = new ProductInbound
            {
                Name = productToUpdate.Name,
                Description = productToUpdate.Description,
                Author = productToUpdate.Author,
                Price = productToUpdate.Price,
                ImageUrl = productToUpdate.ImageUrl
            };

            var updatedProduct = await _productService.UpdateItemById(id, productInbound);
            TempData["Success"] = "Updated successfully!";
            return View(updatedProduct);
        }

        // GET: Products/Delete/{Guid}
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.GetItemById(id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _productService.RemoveItemById(id);
            if (result > 0)
            {
                TempData["Deleted"] = $"Product with id: '{id}' was deleted!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
