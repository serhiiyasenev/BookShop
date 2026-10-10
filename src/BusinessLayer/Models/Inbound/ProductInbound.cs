using System;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Validation;

namespace BusinessLayer.Models.Inbound
{
    public class ProductInbound
    {
        [Required]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 100 characters")]
        public string Name { get; set; }

        [StringLength(1000, MinimumLength = 6, ErrorMessage = "Description must be between 6 and 1000 characters")]
        public string Description { get; set; }

        [StringLength(100, MinimumLength = 5, ErrorMessage = "Author must be between 5 and 100 characters")]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0", "9999999999999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Price must be between 0 and 9999999999999999.99")]
        [PriceScale]
        public decimal Price { get; set; }

        [DataType(DataType.ImageUrl)]
        [StringLength(1000, MinimumLength = 6, ErrorMessage = "URL Length must be between 6 and 1000 characters")]
        public string ImageUrl { get; set; }
    }
}

