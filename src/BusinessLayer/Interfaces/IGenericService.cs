using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
