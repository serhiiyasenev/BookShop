using System;
using System.IO;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IProductService<Inbound, Outbound>
        : IGenericService<Inbound, Outbound> where Inbound : class where Outbound : class
    {
        Task<int> RemoveItemById(Guid id);

        Task<(bool, string)> SaveImage(string path, Stream image);
    }
}
