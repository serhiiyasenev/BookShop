using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class ItemsRequest
    {
        public string ItemName { get; set; }

        [Range(1, 100)]
        public int PageSize { get; set; }

        [Range(1, 10000)]
        public int PageNumber { get; set; }
    }
}
