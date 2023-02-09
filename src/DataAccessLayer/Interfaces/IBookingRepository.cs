﻿using DataAccessLayer.DTO;
using DataAccessLayer.Models;
using System;
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
        /// Get all Bookings by predicate Items Request
        /// </summary>
        /// <returns>
        /// Bookings collection or empty collection
        /// </returns>
        Task<(IQueryable<BookingDto> FilteredItems, int TotalCount)> GetAll(ItemsRequest request);

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
        Task<BookingDto> UpdateById(Guid id, BookingDto booking);

        /// <summary>
        /// Update Booking status by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns>
        /// Updated Booking status or null if Product was not found by id
        /// </returns>
        Task<BookingDto> UpdateStatusById(Guid id, int status);
    }
}
