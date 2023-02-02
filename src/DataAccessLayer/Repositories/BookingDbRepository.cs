using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
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
            var bookingEntity = await dbContext.Bookings.AddAsync(booking);
            await dbContext.SaveChangesAsync();
            return bookingEntity.Entity;
        }

        public IQueryable<BookingDto> GetAll()
        {
            return dbContext.Bookings.AsNoTracking();
        }

        public async Task<BookingDto> GetById(Guid id)
        {
            return await dbContext.Bookings.FindAsync(id);
        }

        public async Task<BookingDto> UpdateById(Guid id, BookingDto booking)
        {
            booking.Id = id;
            dbContext.Attach(booking);
            dbContext.Entry(booking).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return booking;
        }
    }
}