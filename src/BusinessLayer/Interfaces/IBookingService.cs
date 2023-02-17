using BusinessLayer.Enums;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IBookingService
    {
        Task<BookingOutbound> AddItem(BookingInbound item, CancellationToken cancellationToken = default);

        Task<(IQueryable<BookingOutbound> FilteredItems, int TotalCount)> GetAll(RequestModel request, CancellationToken cancellationToken = default);

        Task<BookingOutbound> GetItemById(Guid id);

        Task<BookingOutbound> UpdateItemById(Guid id, BookingInbound item);

        Task<BookingOutbound> UpdateItemStatusById(Guid id, BookingStatus status);
    }
}
