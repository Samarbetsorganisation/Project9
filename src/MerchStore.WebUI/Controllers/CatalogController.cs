using Microsoft.AspNetCore.Mvc;
using MerchStore.Application.Services.Interfaces;
using MerchStore.WebUI.Models.Catalog;
using System; // For Guid
using System.Linq; // For .Select()
using System.Threading.Tasks; // For Task
using MerchStore.Application.Models.ExternalApi; // For ExternalApiProductReviewsResponse

namespace MerchStore.WebUI.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IExternalProductApiService _externalApiService; // Injected
        private readonly IExternalApiTokenProvider _tokenProvider;     // Injected

        public CatalogController(
            ICatalogService catalogService,
            IExternalProductApiService externalApiService,
            IExternalApiTokenProvider tokenProvider)
        {
            _catalogService = catalogService;
            _externalApiService = externalApiService;
            _tokenProvider = tokenProvider;
        }

        // GET: Catalog
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _catalogService.GetAllProductsAsync();
                var productViewModels = products.Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    TruncatedDescription = p.Description != null && p.Description.Length > 100
                        ? p.Description.Substring(0, 97) + "..."
                        : p.Description ?? string.Empty,
                    FormattedPrice = p.Price.ToString(), // Assuming Price has a good ToString()
                    PriceAmount = p.Price.Amount, // Assuming Price is a ValueObject with Amount
                    ImageUrl = p.ImageUrl?.ToString(),
                    StockQuantity = p.StockQuantity
                }).ToList();

                var viewModel = new ProductCatalogViewModel
                {
                    FeaturedProducts = productViewModels
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductCatalog/Index: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading products. Please try again later.";
                return View("Error"); // Assuming you have an Error.cshtml shared view
            }
        }

        // GET: Catalog/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var product = await _catalogService.GetProductByIdAsync(id);

                if (product is null)
                {
                    return NotFound();
                }

                // Map domain entity to view model (initial mapping)
                var viewModel = new ProductDetailsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    FormattedPrice = product.Price.ToString(),
                    PriceAmount = product.Price.Amount,
                    ImageUrl = product.ImageUrl?.ToString(),
                    StockQuantity = product.StockQuantity
                };

                // Fetch and map external reviews
                string? token = await _tokenProvider.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    ExternalApiProductReviewsResponse? reviewsResponse = 
                        await _externalApiService.GetProductReviewsAsync(product.Id.ToString(), token);

                    if (reviewsResponse != null)
                    {
                        if (reviewsResponse.Reviews != null && reviewsResponse.Reviews.Any())
                        {
                            viewModel.Reviews = reviewsResponse.Reviews.Select(r => new ReviewItemViewModel
                            {
                                Date = r.Date, // Consider parsing/formatting if needed
                                Name = r.Name,
                                Rating = r.Rating,
                                Text = r.Text
                            }).ToList();
                        }

                        if (reviewsResponse.Stats != null)
                        {
                            viewModel.AverageRating = reviewsResponse.Stats.CurrentAverage;
                            viewModel.TotalReviews = reviewsResponse.Stats.TotalReviews;
                            viewModel.LastReviewDate = reviewsResponse.Stats.LastReviewDate; // Consider parsing/formatting
                        }
                    }
                }
                
                // Set status message if no reviews were loaded (either API error, timeout, or genuinely no reviews)
                if (viewModel.Reviews == null || !viewModel.Reviews.Any())
                {
                    // Check if there was an attempt to load but it failed, or if there are genuinely no reviews.
                    // For simplicity, if we don't have reviews after the attempt, show the message.
                    viewModel.ReviewStatusMessage = "No reviews available for this product at the moment.";
                }


                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductCatalog/Details for ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading the product details. Please try again later.";
                return View("Error");
            }
        }
    }
}