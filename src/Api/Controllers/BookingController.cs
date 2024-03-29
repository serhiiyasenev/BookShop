﻿using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using InfrastructureLayer.Email.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BookingController : ControllerBase
    {
        private readonly HttpContext _httpContext;
        private readonly ILogger<BookingController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBookingService _bookingService;

        public BookingController(ILogger<BookingController> logger, IHttpContextAccessor contextAccessor,
            IBookingService bookingService, IProductService productService, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            _bookingService = bookingService;
            _httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Create Booking with existing products
        /// </summary>
        /// <param name="bookingInbound"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": "My perfect books for vacation",
        ///       "deliveryAddress": "20 Cooper Square, New York, NY 10003, USA",
        ///       "deliveryDate": "2023-04-03",
        ///       "customerEmail": "email.test@gmail.com",
        ///       "products": [
        ///         "b6226a04-a337-4bbd-d263-08db0a0647bf",
        ///         "07558f14-4680-409e-d264-08db0a0647bf"
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        /// <response code="404">If the product id is incorrect</response>
        /// <response code="409">If the product is already linked to another Booking</response>
        /// <response code="500">Internal Server Error</response>
        /// <remarks>
        /// The returns newly created Booking
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [ProducesResponseType(404, Type = typeof(ProblemDetails))]
        [ProducesResponseType(409, Type = typeof(ProblemDetails))]
        [ProducesResponseType(500, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> AddBooking(BookingInbound bookingInbound)
        {
            try
            {
                var createdBooking = await _bookingService.AddItem(bookingInbound);

                _logger.LogInformation("Booking was created with id: '{BookingId}'", createdBooking.Id);

                await _emailSender.SendEmailAsync(createdBooking.CustomerEmail, "Your booking was created",
                    $"<b> Congratulations! </b> <br> <br> Your booking is: <br> <br> {createdBooking}");

                _logger.LogInformation("Booking email was sent to '{Email}'", createdBooking.CustomerEmail);

                return CreatedAtAction(nameof(AddBooking), createdBooking);
            }
            // it will be updated by adding centralized Exception handling
            catch (Exception ex)
            {
                if (ex.Message.Contains("Not Found"))
                {
                    return NotFound(new ProblemDetails { Title = "Not Found by id", Detail = ex.Message });
                }

                if (ex.Message.Contains("already linked"))
                {
                    return Conflict(new ProblemDetails { Title = "Product already linked", Detail = ex.Message });
                }

                return StatusCode(500, new ProblemDetails { Title = "Internal Server error", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Get all Bookings
        /// </summary>
        /// <remarks>
        /// The returns all Bookings from a storage (selected by RequestModel predicate)
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ResponseModel<BookingOutbound>))]
        public async Task<ActionResult<ResponseModel<BookingOutbound>>> GetAllProducts([FromQuery] RequestModel request, CancellationToken cancellationToken = default)
        {
            var contextTest = _httpContext.User;
            // it will be updated to get results due to UserContext

            var bookings = await _bookingService.GetAll(request, cancellationToken);
            var result = new ResponseModel<BookingOutbound>()
            {
                Items = bookings.FilteredItems,   
                TotalCount = bookings.TotalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
            return Ok(result);
        }

        /// <summary>
        /// Get Booking by id
        /// </summary>
        /// <remarks>
        /// The returns pointed by it's Guid Booking from a storage
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
        /// Update Booking
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookingToUpdate"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": "My perfect books for vacation Update",
        ///       "deliveryAddress": "Update 20 Cooper Square, New York, NY 10003, USA",
        ///       "deliveryDate": "2023-04-03",
        ///       "customerEmail": "email.Update@gmail.com",
        ///       "products": [
        ///         "b6226a04-a337-4bbd-d263-08db0a0647bf",
        ///         "07558f14-4680-409e-d264-08db0a0647bf"
        ///       ]
        ///     }
        ///
        /// The returns newly updated Booking <br/>
        /// It will add new producs if you specified them in producs array <br/>
        /// <b> If you need just update Booking without adding new products, leave `products []` empty here </b> <br/>
        /// (it will not remove previously added products)
        /// </remarks>
        /// <response code="200">Returns the newly updated item</response>
        /// <response code="400">If the item is incorrect</response>
        /// <response code="404">If the product id is incorrect</response>
        /// <response code="409">If the product item in booking is already mapped</response>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        [ProducesResponseType(409, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateBookingById(Guid id, BookingInbound bookingToUpdate)
        {
            var updatedBooking = await _bookingService.UpdateItemById(id, bookingToUpdate);
            return updatedBooking != null ? Ok(updatedBooking) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }

        /// <summary>
        /// Update Booking status by id
        /// </summary>
        /// <remarks>
        /// The returns newly updated Booking
        /// </remarks>
        [HttpPatch("{id}")]
        [ProducesResponseType(200, Type = typeof(SimpleResult))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateBookingStatusById(Guid id, BookingStatus bookingStatus)
        {
            var updatedBooking = await _bookingService.UpdateItemStatusById(id, bookingStatus);

            if (updatedBooking == null) return NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });

            await _emailSender.SendEmailAsync(updatedBooking.CustomerEmail, 
                "Your booking status was updated",
                $"Your booking is: <br> <br> {updatedBooking}");

            return Ok(new SimpleResult { Result = $"Status for booking with id: " +
                $"'{updatedBooking.Id}' was updated to: '{updatedBooking.Status}'" }) ;
        }
    }
}
