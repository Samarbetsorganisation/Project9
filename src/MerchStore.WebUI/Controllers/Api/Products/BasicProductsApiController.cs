using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MerchStore.WebUI.Models.Api.Basic;
using MerchStore.Application.Services.Interfaces;

namespace MerchStore.WebUI.Controllers.Api.Products;

/// <summary>
/// Basic API controller for read-only product operations.
/// Requires API Key authentication.
/// </summary>
[Route("api/basic/products")]
[ApiController]
[Authorize(Policy = "ApiKeyPolicy")]
public class BasicProductsApiController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="catalogService">The catalog service for accessing product data</param>
    public BasicProductsApiController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // The rest of the controller remains unchanged...
}