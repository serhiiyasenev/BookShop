using BusinessLayer.Models.Inbound.Product;
using System.Collections.Generic;

namespace BusinessLayer.Models.Inbound.Booking
{
    public class BookingInboundWithProducts : BookingInboundBase<ProductInbound>
    {
        public override IEnumerable<ProductInbound> Products { get; set; }
    }
}
