using BusinessLayer.Enums;
using System;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IBookingService<Inbound, Outbound> 
        : IGenericService<Inbound, Outbound> where Inbound : class where Outbound : class
    {
        Task<Outbound> UpdateItemStatusById(Guid id, BookingStatus status);
    }
}
