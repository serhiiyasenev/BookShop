using System;
using System.Collections.Generic;

namespace BusinessLayer.Models.Inbound.Booking
{
    public class BookingInboundWithIds : BookingInboundBase<Guid>
    {
        public override IEnumerable<Guid> Products { get; set; }
    }
}