using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.DTO
{
    [Table("Bookings")]
    public class BookingDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string DeliveryAddress { get; set; }

        [Required]
        [MaxLength(128)]
        public string CustomerEmail { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
