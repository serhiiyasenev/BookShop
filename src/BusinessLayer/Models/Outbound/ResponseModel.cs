using System.Collections.Generic;

namespace BusinessLayer.Models.Outbound
{
    public class ResponseModel<T> where T : class
    {
        public IEnumerable<T> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }
}
