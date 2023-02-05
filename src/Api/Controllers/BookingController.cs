using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Inbound.Booking;
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
        private readonly IProductService<ProductInbound, ProductOutbound> _productService;
        private readonly IBookingService<BookingInboundWithProducts, BookingOutbound> _bookingService;

        public BookingController(ILogger<BookingController> logger, 
            IBookingService<BookingInboundWithProducts, BookingOutbound> bookingService, 
            IProductService<ProductInbound, ProductOutbound> productService)
        {
            _logger = logger;
            _productService = productService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Create Booking with new products
        /// </summary>
        /// <param name="booking"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "deliveryAddress": "string",
        ///       "deliveryDate": "2023-12-03",
        ///       "status": "Submitted",
        ///       "products": [
        ///         {
        ///           "name": "string",
        ///           "description": "string",
        ///           "author": "string",
        ///           "price": 12.59,
        ///           "imageUrl": "string"
        ///         }
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> AddBooking(BookingInboundWithProducts booking)
        {
            var createdbooking = await _bookingService.AddItem(booking);
            _logger.LogInformation($"Booking was created with id: '{createdbooking.Id}'");
            return CreatedAtAction(nameof(AddBooking), createdbooking);
        }

        /// <summary>
        /// Create Booking with existing products
        /// </summary>
        /// <param name="bookingWithIds"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "deliveryAddress": "string",
        ///       "deliveryDate": "2023-12-03",
        ///       "status": "Submitted",
        ///       "products": [
        ///         "Guid-1",
        ///         "Guid-2",
        ///         "Guid-3"
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        /// <response code="404">If the product id is incorrect</response>
        [HttpPost]
        [Route("WithExistingProducts")]
        [ProducesResponseType(201, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> AddBookingWithExistingProducts(BookingInboundWithIds bookingWithIds)
        {
            foreach (var id in bookingWithIds.Products)
            {
                if (await _productService.GetItemById(id) == null)
                {
                    NotFound(new SimpleResult { Result = $"NotFound by Product id: '{id}'" });
                }
            }

            var booking = new BookingInboundWithProducts
            {
                DeliveryAddress = bookingWithIds.DeliveryAddress,
                DeliveryDate = bookingWithIds.DeliveryDate,
                Status = bookingWithIds.Status
            };
            var createdbooking = await _bookingService.AddItemWithExistingProducts(booking, bookingWithIds.Products);
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
        public async Task<IActionResult> UpdateBookingById(Guid id, BookingInboundWithProducts booking)
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
