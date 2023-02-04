using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Outbound
{
    public class BookingOutbound
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        public DateOnly CreatedDate { get; set; }

        [EnumDataType(typeof(BookingStatus))]
        public BookingStatus Status { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least 1 Product shoule be added")]
        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public IEnumerable<ProductOutbound> Products { get; set; }
    }
}
