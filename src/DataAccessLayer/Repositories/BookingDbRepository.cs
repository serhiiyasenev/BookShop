using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class BookingDbRepository : BaseDbRepository, IBookingRepository
    {
        public BookingDbRepository(EfCoreContext efContext) : base(efContext)
        {
        }

        public async Task<BookingDto> Add(BookingDto booking, CancellationToken cancellationToken = default)
        {
            var bookingEntity = await _dbContext.Bookings.AddAsync(booking, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return bookingEntity.Entity;
        }

        public async Task<(IQueryable<BookingDto> FilteredItems, int TotalCount)> GetAll(ItemsRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Bookings.Include(e => e.Products).AsNoTracking();
            if (!string.IsNullOrEmpty(request.ItemName))
                query = query.Where(item => item.Name.Contains(request.ItemName));
           
            int totalCount = await query.CountAsync(cancellationToken);
            query = query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
            return (query, totalCount);
        }

        public async Task<BookingDto> GetById(Guid id)
        {
            return await _dbContext.Bookings.AsNoTracking()
                                   .Include(p => p.Products).SingleOrDefaultAsync(b => b.Id.Equals(id));
        }

        public async Task<BookingDto> Update(BookingDto bookingUpdate)
        {
            _dbContext.Attach(bookingUpdate);
            _dbContext.Entry(bookingUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return bookingUpdate;
        }

        public async Task<BookingDto> UpdateStatusById(Guid id, int status)
        {
            var booking = await _dbContext.Bookings.AsNoTracking().Include(p => p.Products).SingleOrDefaultAsync(b => b.Id.Equals(id));
            if (booking == null) return null;

            booking.Status = status;
            _dbContext.Attach(booking);
            _dbContext.Entry(booking).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return booking;
        }
    }
}
