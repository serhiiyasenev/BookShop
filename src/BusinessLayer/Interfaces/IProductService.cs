using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IProductService
    {
        Task<ProductOutbound> AddItem(ProductInbound item);

        Task<(IQueryable<ProductOutbound> FilteredItems, int TotalCount)> GetAll(RequestModel request);

        Task<ProductOutbound> GetItemById(Guid id);

        Task<ProductOutbound> UpdateItemById(Guid id, ProductInbound item);

        Task<int> RemoveItemById(Guid id);

        Task<(bool, string)> SaveImage(string path, Stream image);
    }
}
