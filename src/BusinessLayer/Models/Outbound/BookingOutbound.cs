using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public override string ToString()
        {
            return $"1. Delivery Address: {DeliveryAddress} <br/>" +
                   $"2. Customer Email: {CustomerEmail} <br/>" +
                   $"3. Delivery Date: {DeliveryDate} <br/>" +
                   $"4. Created Date: {CreatedDate.ToString("yyyy-MM-dd")} <br/>" +
                   $"5. Booking Status: {Status} <br/>" +
                   $"6. Products: {Products.Count()}";
        }
    }
}
