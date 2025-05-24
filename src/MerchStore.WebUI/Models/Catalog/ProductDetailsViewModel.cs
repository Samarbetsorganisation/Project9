using System;
using System.Collections.Generic;

namespace MerchStore.WebUI.Models.Catalog
{
    // New ViewModel for a single review item to be displayed
    public class ReviewItemViewModel
    {
        public string? Date { get; set; }
        public string? Name { get; set; }
        public int Rating { get; set; }
        public string? Text { get; set; }
        // Optional: Could add a property to generate star icons based on Rating for easier display in Razor
        // public string StarRatingHtml { get; /* set based on Rating */ } 
    }

    public class ProductDetailsViewModel
    {
        // Existing properties
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FormattedPrice { get; set; } = string.Empty;
        public decimal PriceAmount { get; set; }
        public string? ImageUrl { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
        public bool InStock => StockQuantity > 0;
        public int StockQuantity { get; set; }

        // New properties for reviews and stats
        public List<ReviewItemViewModel>? Reviews { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public string? LastReviewDate { get; set; } // Or DateTime, depending on display needs
        
        public string? ReviewStatusMessage { get; set; } // To display "No reviews available..." or similar
    }
}