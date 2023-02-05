using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IGenericService<Inbound, Outbound> where Inbound : class where Outbound : class
    {
        Task<Outbound> AddItem(Inbound item);

        IQueryable<Outbound> GetAllItems();

        Task<Outbound> GetItemById(Guid id);

        Task<Outbound> UpdateItemById(Guid id, Inbound item);
    }
}
