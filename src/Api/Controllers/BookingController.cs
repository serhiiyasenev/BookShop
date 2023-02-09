using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using InfrastructureLayer.Email.Interfaces;
using Microsoft.AspNetCore.Http;
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
        private readonly HttpContext _httpContext;
        private readonly ILogger<BookingController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IProductService _productService;
        private readonly IBookingService _bookingService;

        public BookingController(ILogger<BookingController> logger, IHttpContextAccessor contextAccessor,
            IBookingService bookingService, IProductService productService, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            _productService = productService;
            _bookingService = bookingService;
            _httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Create Booking with existing products endpoint
        /// </summary>
        /// <param name="bookingInbound"></param>
        /// <returns>A newly created Booking item</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "deliveryAddress": "20 Cooper Square, New York, NY 10003, USA",
        ///       "deliveryDate": "2023-04-03",
        ///       "customerEmail": "email.test@gmail.com",
        ///       "products": [
        ///         "b6226a04-a337-4bbd-d263-08db0a0647bf",
        ///         "07558f14-4680-409e-d264-08db0a0647bf",
        ///         "7794d821-035f-454d-8a1c-12b085ef5917"
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is incorrect</response>
        /// <response code="404">If the product id is incorrect</response>
        /// <remarks>
        /// The endpoint returns newly created Booking
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(BookingOutbound))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        [ProducesResponseType(500, Type = typeof(SimpleResult))]
        public async Task<IActionResult> AddBooking(BookingInbound bookingInbound)
        {
            try
            {
                var createdBooking = await _bookingService.AddItem(bookingInbound);

                _logger.LogInformation($"Booking was created with id: '{createdBooking.Id}'");

                await _emailSender.SendEmailAsync(createdBooking.CustomerEmail, "Your booking was created",
                    $"<b> Congratulations! </b> <br> <br> Your booking is: <br> <br> {createdBooking}");

                _logger.LogInformation($"Booking email was sent to `{createdBooking.CustomerEmail}`'");

                return CreatedAtAction(nameof(AddBooking), createdBooking);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Not found"))
                {
                    return NotFound(new SimpleResult { Result = ex.Message });
                }
                else
                {
                    return StatusCode(500, new SimpleResult { Result = ex.Message });
                }
                
            }
        }

        /// <summary>
        /// Get all Bookings endpoint
        /// </summary>
        /// <remarks>
        /// The endpoint returns all Bookings from a storage (selected by RequestModel predicate)
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ResponseModel<BookingOutbound>))]
        public async Task<ActionResult<ResponseModel<BookingOutbound>>> GetAllProducts([FromQuery] RequestModel request)
        {
            var contextTest = _httpContext.User;
            // it will be updated to get results due to Context

            var bookings = await _bookingService.GetAll(request);
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
        /// Get Booking by id endpoint
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
        /// The endpoint returns newly updated Booking <br/>
        /// It will add new producs if you specified them in producs array <br/>
        /// <b> If you need just update Booking without adding new products, skip `products []` here</b>
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(BookingOutbound))]
        [ProducesResponseType(404, Type = typeof(SimpleResult))]
        public async Task<IActionResult> UpdateBookingById(Guid id, BookingInbound booking)
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
        public async Task<IActionResult> UpdateBookingStatusById(Guid id, BookingStatus bookingStatus)
        {
            var updatedBooking = await _bookingService.UpdateItemStatusById(id, bookingStatus);
            await _emailSender.SendEmailAsync(updatedBooking.CustomerEmail, 
                "Your booking status was updated",
                $"Your booking is: <br> <br> {updatedBooking}");
            return updatedBooking != null ? Ok(updatedBooking) : NotFound(new SimpleResult { Result = $"NotFound by id: '{id}'" });
        }
    }
}
