using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Inbound
{
    public class ProductInbound
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 100 characters")]
        public string Name { get; set; }

        [StringLength(1000, MinimumLength = 6, ErrorMessage = "Description must be between 6 and 1000 characters")]
        public string Description { get; set; }

        [StringLength(100, MinimumLength = 5, ErrorMessage = "Author must be between 5 and 100 characters")]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(0, float.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public float Price { get; set; }

        [DataType(DataType.ImageUrl)]
        [Url(ErrorMessage = "Image URL must be a valid URL")]
        [StringLength(1000, MinimumLength = 6, ErrorMessage = "URL Length must be between 6 and 1000 characters")]
        public string ImageUrl { get; set; }
    }
}
