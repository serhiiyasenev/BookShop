using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.Inbound
{
    public class RequestModel
    {
        /// <summary>
        /// Name of items to search by contains
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Page size (default is 25)
        /// </summary>  
        [Range(1, 100)]
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Page number (default is 1 (started from first page))
        /// </summary>
        [Range(1, 10000)]
        public int Page { get; set; } = 1;
    }
}
