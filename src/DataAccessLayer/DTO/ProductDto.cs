using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.DTO
{
    [Table("Products")]
    public class ProductDto
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Description { get; set; }

        [StringLength(50, MinimumLength = 5, ErrorMessage = "Author must be between 5 and 100 characters")]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(0, float.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public float Price { get; set; }

        [DataType(DataType.ImageUrl)]
        [Url(ErrorMessage = "Image URL must be a valid URL")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "URL Length must be between 10 and 100 characters")]
        public string ImageUrl { get; set; }

        [ForeignKey("BookingDto")]
        public Guid? BookingDtoId { get; set; }
    }
}
