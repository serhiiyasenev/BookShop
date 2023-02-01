using BusinessLayer.Enums;
using DataAccessLayer.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BusinessLayer.Models.Outbound
{
    public class BookingOutbound : BaseOutbound
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least 1 Product shoule be added")]
        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public IEnumerable<ProductDto> Products { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }

        private DateOnly _deliveryDate;

        [Required]
        public DateOnly DeliveryDate
        {
            get => _deliveryDate;
            set
            {
                if (!DateOnly.TryParse(value.ToString(CultureInfo.InvariantCulture), out _deliveryDate))
                {
                    throw new ArgumentException($"Cannot parse BirthDate from `{value}`");
                }
                if (_deliveryDate < CreatedDate)
                {
                    throw new ArgumentException($"`DeliveryDate {_deliveryDate}`cannot be before `{CreatedDate}`");
                }
            }
        }

        public DateOnly CreatedDate { get; set; }

        [EnumDataType(typeof(BookingStatus))]
        public BookingStatus Status { get; set; }
    }
}
