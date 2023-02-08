using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Inbound.Booking;
using BusinessLayer.Models.Inbound.Product;
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
        private readonly IProductService<ProductInbound, ProductOutbound> _productService;
        private readonly IBookingService<BookingInboundWithProducts, BookingOutbound> _bookingService;

        public BookingController(ILogger<BookingController> logger, IHttpContextAccessor contextAccessor,
            IBookingService<BookingInboundWithProducts, BookingOutbound> bookingService, 
            IProductService<ProductInbound, ProductOutbound> productService, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            _productService = productService;
            _bookingService = bookingService;
            _httpContext = contextAccessor.HttpContext;
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
        ///       "deliveryAddress": "20 Cooper Square, New York, NY 10003, USA",
        ///       "deliveryDate": "2023-04-03",
        ///       "customerEmail": "email.test+1@gmail.com",
        ///       "products": [
        ///         {
        ///           "name": "MSDN Edition 1",
        ///           "description": "MSDN was developed for managing the firm's relationship with developers and testers",
        ///           "author": "John Doe",
        ///           "price": 12.34,
        ///           "imageUrl": "ftp://book.shop/downloads/image.jpg"
        ///         },
        ///         {
        ///           "name": "MSDN Edition 2",
        ///           "description": "This is a description 2 - short option",
        ///           "author": "John Doe II",
        ///           "price": 22.34,
        ///           "imageUrl": "ftp://book.shop/downloads/image2.jpg"
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
            var createdBooking = await _bookingService.AddItem(booking);
            _logger.LogInformation($"Booking was created with id: '{createdBooking.Id}'");

            await _emailSender.SendEmailAsync(createdBooking.CustomerEmail, "Your booking was created",
                $"<b> Congratulations! </b> <br> <br> Your booking is: <br> <br> {createdBooking}");

            _logger.LogInformation($"Booking email was sent to `{createdBooking.CustomerEmail}`'");
            return CreatedAtAction(nameof(AddBooking), createdBooking);
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
        ///       "deliveryAddress": "20 Cooper Square, New York, NY 10003, USA",
        ///       "deliveryDate": "2023-04-03",
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
                    return NotFound(new SimpleResult { Result = $"NotFound by Product id: '{id}'" });
                }
            }

            var booking = new BookingInboundWithProducts
            {
                DeliveryAddress = bookingWithIds.DeliveryAddress,
                DeliveryDate = bookingWithIds.DeliveryDate
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
        [ProducesResponseType(200, Type = typeof(ResponseModel<BookingOutbound>))]
        public ActionResult<ResponseModel<BookingOutbound>> GetAllProducts([FromQuery] GetItemsRequest request)
        {
            var tequestTest = request;
            var contextTest = _httpContext;
            // it will be updated to get results via predicates From Query string and Context
            var bookings = _bookingService.GetAllItems();
            var result = new ResponseModel<BookingOutbound>()
            {
                Items = bookings,   
                TotalCount = bookings.Count()
            };
            return Ok(result);
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
        /// The endpoint returns newly updated Booking <br/>
        /// It will add new producs if you specified them in producs array <br/>
        /// <b> If you need just update Booking without adding new products, skip `products []` here</b>
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
