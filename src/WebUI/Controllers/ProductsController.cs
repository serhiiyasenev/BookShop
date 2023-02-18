using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using BusinessLayer.Models.Inbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

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
            (System.Linq.IQueryable<BusinessLayer.Models.Outbound.ProductOutbound> FilteredItems, int TotalCount) products = await _productService.GetAll(requestModel);
            return View(products);
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
        public async Task<IActionResult> Edit(Guid id, ProductInbound productInbound)
        {
            var product = await _productService.GetItemById(id);
            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updatedProduct = await _productService.UpdateItemById(id, productInbound);
                return RedirectToAction(nameof(Index));
            }

            return View(productInbound);
        }

        // GET: Products/Delete/{Guid}
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.GetItemById(id);

            if (result == null)
            {
                return NotFound();
            }

            return View();
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _productService.RemoveItemById(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
