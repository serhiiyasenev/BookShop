using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Outbound
{
    public class BookingOutbound
    {
        public Guid Id { get; set; }

        public string DeliveryAddress { get; set; }

        public string CustomerEmail { get; set; }

        public DateOnly DeliveryDate { get; set; }

        public DateTime CreatedDate { get; set; }

        [EnumDataType(typeof(BookingStatus))]
        public BookingStatus Status { get; set; }

        public IEnumerable<ProductOutbound> Products { get; set; }
    }
}
