using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace BusinessLayer.Models.Inbound
{
    public class BookingInbound
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Booking Name must be between 5 and 100 characters")]
        public string Name { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "Delivery Addresse must be between 6 and 100 characters")]
        public string DeliveryAddress { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Customer Email must be between 6 and 100 characters")]
        public string CustomerEmail { get; set; }

        [JsonIgnore]
        [ReadOnly(true)]
        public DateTime CreatedDate => DateTime.UtcNow;

        [JsonIgnore]
        [ReadOnly(true)]
        public BookingStatus Status => BookingStatus.Submitted;

        private DateOnly _deliveryDate;

        [Required]
        public DateOnly DeliveryDate
        {
            get => _deliveryDate;
            set
            {
                if (!DateOnly.TryParse(value.ToString(CultureInfo.InvariantCulture), out _deliveryDate))
                {
                    throw new ArgumentException($"Cannot parse DeliveryDate from `{value}`");
                }
                if (_deliveryDate < DateOnly.FromDateTime(CreatedDate))
                {
                    throw new ArgumentException($"`DeliveryDate {_deliveryDate}`cannot be before `{CreatedDate}`");
                }
            }
        }

        [Required]
        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public IEnumerable<Guid> Products { get; set; }
    }
}
