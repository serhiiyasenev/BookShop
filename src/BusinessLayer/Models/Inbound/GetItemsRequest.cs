using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Inbound
{
    public class GetItemsRequest
    {
        public string ItemName { get; set; }

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [Range(1, 10000)]
        public int Page { get; set;} = 1;
    }
}
