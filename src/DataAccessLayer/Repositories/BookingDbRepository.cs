using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class BookingDbRepository : BaseDbRepository, IBookingRepository
    {
        public BookingDbRepository(EfCoreContext efContext) : base(efContext)
        {
        }

        public async Task<BookingDto> Add(BookingDto booking)
        {
            //booking.Id = Guid.NewGuid();
            var bookingEntity = await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();
            return bookingEntity.Entity;
        }

        public async Task<(IQueryable<BookingDto> FilteredItems, int TotalCount)> GetAll(ItemsRequest request)
        {
            var query = _dbContext.Bookings.AsNoTracking();
            if (!string.IsNullOrEmpty(request.ItemName))
            {
                query = query.Where(item => item.Name.Contains(request.ItemName));
            }
            // bottleneck ??
            int totalCount = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
            return (query, totalCount);
        }

        public async Task<BookingDto> GetById(Guid id)
        {
            return await _dbContext.Bookings.AsNoTracking().Include(p => p.Products).SingleOrDefaultAsync(b => b.Id.Equals(id));
        }

        public async Task<BookingDto> UpdateById(Guid id, BookingDto bookingUpdate)
        {
            var bookingDb = await _dbContext.Bookings.AsNoTracking()
                .Include(p => p.Products).SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (bookingDb == null)
            {
                return null;
            }

            if (bookingUpdate.Products.Count() > 0)
            {
                var dbProducts = bookingDb.Products.ToList();
                dbProducts.AddRange(bookingUpdate.Products);
                bookingDb.Products = dbProducts;
            }

            bookingDb.DeliveryDate = bookingUpdate.DeliveryDate;
            bookingDb.DeliveryAddress = bookingUpdate.DeliveryAddress;
            bookingDb.CustomerEmail = bookingUpdate.CustomerEmail;

            _dbContext.Attach(bookingDb);
            _dbContext.Entry(bookingDb).State = EntityState.Modified;
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return bookingDb;
        }

        public async Task<BookingDto> UpdateStatusById(Guid id, int status)
        {
            var booking = await _dbContext.Bookings.AsNoTracking().Include(p => p.Products).SingleOrDefaultAsync(b => b.Id.Equals(id));
            if (booking == null)
            {
                return null;
            }
            booking.Status = status;
            _dbContext.Attach(booking);
            _dbContext.Entry(booking).State = EntityState.Modified;
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return booking;
        }
    }
}
