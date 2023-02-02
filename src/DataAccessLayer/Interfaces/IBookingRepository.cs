using DataAccessLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IBookingRepository
    {
        /// <summary>
        /// Add Booking
        /// </summary>
        /// <param name="booking"></param>
        /// <returns>
        /// Newly created Booking
        /// </returns>
        Task<BookingDto> Add(BookingDto booking);

        /// <summary>
        /// Get all Bookings
        /// </summary>
        /// <returns>
        /// Bookings collection or empty collection
        /// </returns>
        IQueryable<BookingDto> GetAll();

        /// <summary>
        /// Get Booking by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Booking with pointed ID or null if Booking was not found by ID
        /// </returns>
        Task<BookingDto> GetById(Guid id);

        /// <summary>
        /// Update Booking by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="booking"></param>
        /// <returns>
        /// Updated Booking or null if Product was not found by id
        /// </returns>
        Task<BookingDto> UpdateById(Guid Guid, BookingDto booking);
    }
}
