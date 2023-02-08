using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
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
            booking.Id = Guid.NewGuid();
            var bookingEntity = await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();
            return bookingEntity.Entity;
        }

        public async Task<BookingDto> AddWithExistingProducts(BookingDto booking, IEnumerable<Guid> ids)
        {
            booking.Id = Guid.NewGuid();
            var dbProducts = await _dbContext.Products.ToListAsync();
            booking.Products = dbProducts.Where(e => ids.Contains(e.Id)).ToList();
            var bookingEntity = await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();
            return bookingEntity.Entity;
        }

        public IQueryable<BookingDto> GetAll()
        {
            return _dbContext.Bookings.AsNoTracking();
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
