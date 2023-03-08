using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace WebUI.Models.Products
{
    public class IndexModel : PageModel
    {
        public IQueryable<ProductOutbound> FilteredItems { get; set; }

        public int TotalCount { get; set; }

        public RequestModel RequestModel { get; set; }

        public IEnumerable<SelectListItem> PageSizeSelectList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "10", Value = "10" },
            new SelectListItem { Text = "20", Value = "20" },
            new SelectListItem { Text = "25", Value = "25" }
        };
    }
}