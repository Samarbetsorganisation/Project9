using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;
using MerchStore.WebUI.Models.Admin;

namespace MerchStore.WebUI.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IProductAdminService _productAdminService;
        private readonly IImageUploadService _imageUploadService;

        public AdminProductsController(
            IProductAdminService productAdminService,
            IImageUploadService imageUploadService)
        {
            _productAdminService = productAdminService;
            _imageUploadService = imageUploadService;
        }

        // GET: /Admin/AdminProducts
        public async Task<IActionResult> Index()
        {
            var products = await _productAdminService.GetAllAsync();
            return View("~/Views/Admin/AdminProducts/Index.cshtml", products);
        }

        // GET: /Admin/AdminProducts/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productAdminService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var vm = MapToViewModel(product);
            return View("~/Views/Admin/AdminProducts/Details.cshtml", vm);
        }

        // GET: /Admin/AdminProducts/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/AdminProducts/Create.cshtml", new AdminProductViewModel());
        }

        // POST: /Admin/AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/AdminProducts/Create.cshtml", vm);

            string? imageUrl = null;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                using (var stream = vm.ImageFile.OpenReadStream())
                {
                    imageUrl = await _imageUploadService.UploadImageAsync(
                        stream,
                        vm.ImageFile.FileName,
                        vm.ImageFile.ContentType
                    );
                }
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Product image is required.");
                return View("~/Views/Admin/AdminProducts/Create.cshtml", vm);
            }

            var tags = ParseTags(vm.Tags);

            // Map ViewModel to domain Product entity
            var product = new Product(
                vm.Name,
                vm.Description,
                new Uri(imageUrl ?? string.Empty),
                Money.FromSEK(vm.PriceAmount),
                vm.StockQuantity,
                vm.Category,
                tags
            );

            await _productAdminService.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminProducts/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productAdminService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var vm = MapToViewModel(product);
            return View("~/Views/Admin/AdminProducts/Edit.cshtml", vm);
        }

        // POST: /Admin/AdminProducts/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AdminProductViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/AdminProducts/Edit.cshtml", vm);

            var existingProduct = await _productAdminService.GetByIdAsync(id);
            if (existingProduct == null)
                return NotFound();

            Uri? imageUrl = existingProduct.ImageUrl;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                using (var stream = vm.ImageFile.OpenReadStream())
                {
                    var uploadedUrl = await _imageUploadService.UploadImageAsync(
                        stream,
                        vm.ImageFile.FileName,
                        vm.ImageFile.ContentType
                    );
                    imageUrl = new Uri(uploadedUrl);
                }
            }

            var tags = ParseTags(vm.Tags);

            // Use domain logic to update
            existingProduct.UpdateDetails(
                vm.Name,
                vm.Description,
                imageUrl,
                vm.Category,
                tags
            );
            existingProduct.UpdatePrice(Money.FromSEK(vm.PriceAmount));
            existingProduct.UpdateStock(vm.StockQuantity);

            var success = await _productAdminService.UpdateAsync(existingProduct);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminProducts/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productAdminService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var vm = MapToViewModel(product);
            return View("~/Views/Admin/AdminProducts/Delete.cshtml", vm);
        }

        // POST: /Admin/AdminProducts/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _productAdminService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --------- Mapping Helpers ---------

        private static AdminProductViewModel MapToViewModel(Product product)
        {
            return new AdminProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PriceAmount = product.Price.Amount,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                Tags = product.Tags != null ? string.Join(", ", product.Tags) : "",
                ExistingImageUrl = product.ImageUrl?.ToString()
            };
        }

        private static List<string> ParseTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return new List<string>();
            return tags.Split(',')
                       .Select(t => t.Trim())
                       .Where(t => !string.IsNullOrEmpty(t))
                       .ToList();
        }
    }
}