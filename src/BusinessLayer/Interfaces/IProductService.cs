using BusinessLayer.Models.Outbound;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IProductService<Inbound, Outbound>
        : IGenericService<Inbound, Outbound> where Inbound : class where Outbound : class
    {
        Task<int> RemoveItemById(Guid id);
    }
}
