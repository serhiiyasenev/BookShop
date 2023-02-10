using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IProductService
    {
        Task<ProductOutbound> AddItem(ProductInbound item, CancellationToken cancellationToken = default);

        Task<(IQueryable<ProductOutbound> FilteredItems, int TotalCount)> GetAll(RequestModel request, CancellationToken cancellationToken = default);

        Task<ProductOutbound> GetItemById(Guid id);

        Task<ProductOutbound> UpdateItemById(Guid id, ProductInbound item);

        Task<int> RemoveItemById(Guid id);
    }
}
