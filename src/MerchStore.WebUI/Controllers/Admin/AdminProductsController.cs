using System;
using System.Collections.Generic; // Added for List
using System.IO; // Added for Path.GetExtension
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;
using MerchStore.WebUI.Models.Admin;
using Microsoft.Extensions.Logging; // Recommended for logging

namespace MerchStore.WebUI.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IProductAdminService _productAdminService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<AdminProductsController> _logger; // For logging

        public AdminProductsController(
            IProductAdminService productAdminService,
            IImageUploadService imageUploadService,
            ILogger<AdminProductsController> logger) // Inject logger
        {
            _productAdminService = productAdminService;
            _imageUploadService = imageUploadService;
            _logger = logger;
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

            string? newImageUrlString = null;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                try
                {
                    using (var stream = vm.ImageFile.OpenReadStream())
                    {
                        // Generate a unique filename to prevent collisions
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                        newImageUrlString = await _imageUploadService.UploadImageAsync(
                            stream,
                            uniqueFileName,
                            vm.ImageFile.ContentType
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image during product creation for {ProductName}", vm.Name);
                    ModelState.AddModelError("ImageFile", "There was an error uploading the image. Please try again.");
                    return View("~/Views/Admin/AdminProducts/Create.cshtml", vm);
                }
            }
            else
            {
                // This check is already in your AdminProductViewModel via [Required] if you added it,
                // but controller-side check is fine as a fallback or for specific logic.
                // If ImageFile is truly required, ModelState.IsValid should catch it.
                // The model error for ImageFile is handled by ModelState.IsValid.
            }

            var tags = ParseTags(vm.Tags);

            var product = new Product(
                vm.Name,
                vm.Description,
                string.IsNullOrWhiteSpace(newImageUrlString) ? null : new Uri(newImageUrlString),
                Money.FromSEK(vm.PriceAmount),
                vm.StockQuantity,
                vm.Category, // Ensure vm.Category is not null if Product constructor requires it
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

            string? oldImageUrlString = existingProduct.ImageUrl?.ToString();
            Uri? finalImageUrl = existingProduct.ImageUrl; // Start with the current image URL

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                string? uploadedUrlString = null;
                try
                {
                    using (var stream = vm.ImageFile.OpenReadStream())
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                        uploadedUrlString = await _imageUploadService.UploadImageAsync(
                            stream,
                            uniqueFileName,
                            vm.ImageFile.ContentType
                        );
                    }
                    finalImageUrl = new Uri(uploadedUrlString);

                    // If new image uploaded successfully and an old image existed, delete the old one
                    if (!string.IsNullOrWhiteSpace(oldImageUrlString) && oldImageUrlString != uploadedUrlString)
                    {
                        try
                        {
                            await _imageUploadService.DeleteImageAsync(oldImageUrlString);
                        }
                        catch (Exception ex)
                        {
                            // Log the error, but don't let it block the product update
                            _logger.LogWarning(ex, "Error deleting old image {OldImageUrl} during product edit for {ProductId}", oldImageUrlString, existingProduct.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading new image during product edit for {ProductId}", existingProduct.Id);
                    ModelState.AddModelError("ImageFile", "There was an error uploading the new image. The old image has been kept.");
                    // Keep finalImageUrl as the existingProduct.ImageUrl if upload fails
                    finalImageUrl = existingProduct.ImageUrl;
                    // Optionally, you might want to return the view here if image upload is critical
                    // return View("~/Views/Admin/AdminProducts/Edit.cshtml", vm);
                }
            }

            var tags = ParseTags(vm.Tags);

            existingProduct.UpdateDetails(
                vm.Name,
                vm.Description,
                finalImageUrl, // Use the determined final image URL
                vm.Category,
                tags
            );
            existingProduct.UpdatePrice(Money.FromSEK(vm.PriceAmount));
            existingProduct.UpdateStock(vm.StockQuantity);

            var success = await _productAdminService.UpdateAsync(existingProduct);
            if (!success)
            {
                // This case might occur if, for example, optimistic concurrency fails.
                ModelState.AddModelError(string.Empty, "The product could not be updated. It may have been modified or deleted by another user.");
                return View("~/Views/Admin/AdminProducts/Edit.cshtml", vm);
            }

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
            var productToDelete = await _productAdminService.GetByIdAsync(id);
            string? imageUrlToDelete = null;

            if (productToDelete != null)
            {
                imageUrlToDelete = productToDelete.ImageUrl?.ToString();
            }
            else
            {
                // Product doesn't exist or was already deleted.
                return RedirectToAction(nameof(Index)); // Or NotFound()
            }

            // Attempt to delete the product from the database
            var deleteSuccess = await _productAdminService.DeleteAsync(id); // Assuming this returns bool or throws

            if (deleteSuccess) // Or if DeleteAsync is void and doesn't throw, assume success
            {
                if (!string.IsNullOrWhiteSpace(imageUrlToDelete))
                {
                    try
                    {
                        await _imageUploadService.DeleteImageAsync(imageUrlToDelete);
                    }
                    catch (Exception ex)
                    {
                        // Log the error. The product is deleted, but the image cleanup failed.
                        _logger.LogError(ex, "Error deleting image {ImageUrl} for successfully deleted product {ProductId}", imageUrlToDelete, id);
                        // This might require manual cleanup or a background job for orphaned images.
                    }
                }
            }
            else
            {
                // Handle the case where product deletion from DB failed, if applicable
                _logger.LogError("Failed to delete product {ProductId} from the database.", id);
                // You might want to add a TempData message for the user.
                TempData["ErrorMessage"] = "Could not delete the product. Please try again or contact support.";
            }

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
                Category = product.Category ?? string.Empty, // Ensure not null if model expects string
                Tags = product.Tags != null ? string.Join(", ", product.Tags) : string.Empty,
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