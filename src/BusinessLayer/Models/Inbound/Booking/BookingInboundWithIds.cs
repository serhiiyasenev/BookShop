using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Inbound.Booking
{
    public class BookingInboundWithIds : BookingInboundBase<Guid>
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least 1 Product Id shoule be added")]
        public override IEnumerable<Guid> Products { get; set; }
    }
}
