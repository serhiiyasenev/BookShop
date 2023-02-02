﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.DTO
{
    [Table("Bookings")]
    public class BookingDto
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least 1 Product shoule be added")]
        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public IEnumerable<ProductDto> Products { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }

        public DateOnly CreatedDate { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        public int Status { get; set; }
    }
}
