using System;
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
        [Range(0, float.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public float Price { get; set; }

        [MaxLength(1000)]
        public string ImageUrl { get; set; }

        [ForeignKey("BookingDto")]
        public Guid? BookingDtoId { get; set; }
    }
}
