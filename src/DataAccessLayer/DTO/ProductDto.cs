using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.DTO
{
    [Table("Products")]
    public class ProductDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0", "9999999999999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Price must be between 0 and 9999999999999999.99")]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string ImageUrl { get; set; }

        [ForeignKey("BookingDto")]
        public Guid? BookingDtoId { get; set; }
    }
}

