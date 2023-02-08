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
            var products = $"{ string.Join("<br>", Products.Select(e => e.ToString())) }";

            return $"1. Delivery Address: <b> {DeliveryAddress} </b> <br>" +
                   $"2. Delivery Date: <b> {DeliveryDate:dd-MMMM-yyyy} </b> <br>" +
                   $"3. Created Date: {CreatedDate:dd-MMMM-yyyy} <br>" +
                   $"4. Booking Status: {Status} <br>" +
                   $"5. Products: <br> {products}";
        }
    }
}
