using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class BookingService : IBookingService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IMapper mapper, IBookingRepository bookingRepository)
        {
            _mapper = mapper;
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingOutbound> AddItem(BookingInbound booking, CancellationToken cancellationToken = default)
        {
            var products = new List<ProductDto>();
            foreach (var id in booking.Products)
            {
                var product = await _productRepository.GetById(id);
                if (product == null)
                {
                   throw new Exception(message: $"Product Not Found by id: '{id}'");
                }
                else
                {
                    products.Add(product);
                }
            }

            var bookingDto = new BookingDto
            {
                Name            = booking.Name,
                CustomerEmail   = booking.CustomerEmail,
                CreatedDate     = booking.CreatedDate,
                DeliveryAddress = booking.DeliveryAddress,
                DeliveryDate    = booking.DeliveryDate,
                Status          = (int) booking.Status, 
                Products        = products
            };

            var dbItem = await _bookingRepository.Add(bookingDto);
            return _mapper.Map<BookingOutbound>(dbItem);
        }

        public async Task<(IQueryable<BookingOutbound> FilteredItems, int TotalCount)> GetAll(RequestModel request, CancellationToken cancellationToken = default)
        {
            var result = await _bookingRepository.GetAll(_mapper.Map<ItemsRequest>(request));
            return (_mapper.ProjectTo<BookingOutbound>(result.FilteredItems), result.TotalCount);
        }

        public async Task<BookingOutbound> GetItemById(Guid id)
        {
            var dbItem = await _bookingRepository.GetById(id);
            return _mapper.Map<BookingOutbound>(dbItem);
        }

        public async Task<BookingOutbound> UpdateItemById(Guid id, BookingInbound booking)
        {
            var dbItem = await _bookingRepository.UpdateById(id, _mapper.Map<BookingDto>(booking));
            return _mapper.Map<BookingOutbound>(dbItem);
        }

        public async Task<BookingOutbound> UpdateItemStatusById(Guid id, BookingStatus status)
        {
            var dbItem = await _bookingRepository.UpdateStatusById(id, (int)status);
            return _mapper.Map<BookingOutbound>(dbItem);
        }
    }
}
