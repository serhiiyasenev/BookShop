using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Outbound
{
    public class ProductOutbound
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public float Price { get; set; }

        public string ImageUrl { get; set; }

        public Guid? BookingId { get; set; }

        public override string ToString()
        {
            return $"- {Name}, author: {Author}, price: {Price};";
        }
    }
}
