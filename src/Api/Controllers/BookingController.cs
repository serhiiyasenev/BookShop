using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingService<BookingInbound, BookingOutbound> _bookingService;

        public BookingController(ILogger<BookingController> logger, IBookingService<BookingInbound, BookingOutbound> bookingService)
        {
            _logger = logger;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Create Booking
        /// </summary>
        /// <param name="booking"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///     "name": "The Three Musketeers",
        ///     "description": "You have likely heard of The Three Musketeers! This story has been reproduced into films, TV series, and other novels..." ,
        ///     "author": "Alexandre Dumas",
        ///     "price": 12.50,
        ///     "imageUrl": "ftp://book.shop/downloads/image.jpg"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> AddBooking(BookingInbound booking)
        {
            var createdbooking = await _bookingService.AddItem(booking);
            _logger.LogInformation($"Booking was created with id: '{createdbooking.Id}'");
            return CreatedAtAction(nameof(AddBooking), createdbooking);
        }

        /// <summary>
        /// Get all Bookings
        /// </summary>
        /// <remarks>
        /// The endpoint returns all Bookings from a storage
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IQueryable<BookingOutbound>))]
        public IActionResult GetAllBookings()
        {
            return Ok(_bookingService.GetAllItems());
        }

        /// <summary>
        /// Get Booking by id
        /// </summary>
        /// <remarks>
        /// The endpoint returns pointed by it's Guid Booking from a storage
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(BookingOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var booking = await _bookingService.GetItemById(id);
            return booking != null ? Ok(booking) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }

        /// <summary>
        /// Update Booking by id
        /// </summary>
        /// <remarks>
        /// The endpoint returns newly updated Booking
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(BookingOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateProductById(Guid id, BookingInbound booking)
        {
            var updatedBooking = await _bookingService.UpdateItemById(id, booking);
            return updatedBooking != null ? Ok(updatedBooking) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }

        /// <summary>
        /// Update Booking status by id
        /// </summary>
        /// <remarks>
        /// The endpoint returns newly updated Booking
        /// </remarks>
        [HttpPatch("{id}")]
        [ProducesResponseType(200, Type = typeof(BookingOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateProductById(Guid id, BookingStatus bookingStatus)
        {
            var updatedBooking = await _bookingService.UpdateItemStatusById(id, bookingStatus);
            return updatedBooking != null ? Ok(updatedBooking) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }
    }
}
