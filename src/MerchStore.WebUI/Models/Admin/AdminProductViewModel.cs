using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MerchStore.WebUI.Models.Admin
{
    public class AdminProductViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal PriceAmount { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        // For comma-separated tags in the UI
        public string? Tags { get; set; }

        // For displaying the current image
        public string? ExistingImageUrl { get; set; }

        // For uploading a new image
        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }
    }
}