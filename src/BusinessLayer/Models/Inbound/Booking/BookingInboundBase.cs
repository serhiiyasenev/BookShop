﻿using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BusinessLayer.Models.Inbound.Booking
{
    public abstract class BookingInboundBase<T>
    {
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [ReadOnly(true)]
        internal DateTime CreatedDate => DateTime.UtcNow;

        [ReadOnly(true)]
        internal BookingStatus Status => BookingStatus.Submitted;

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

        [MaxLength(100, ErrorMessage = "More than 100 Product are not allowed to add to one order")]
        public abstract IEnumerable<T> Products { get; set; }
    }
}
