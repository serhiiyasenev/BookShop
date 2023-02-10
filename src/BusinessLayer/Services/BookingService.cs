using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
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

        public BookingService(IMapper mapper, IBookingRepository bookingRepository, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingOutbound> AddItem(BookingInbound booking, CancellationToken cancellationToken = default)
        {
            var bookingDto = new BookingDto
            {
                Name            = booking.Name,
                CustomerEmail   = booking.CustomerEmail,
                CreatedDate     = booking.CreatedDate,
                DeliveryAddress = booking.DeliveryAddress,
                DeliveryDate    = booking.DeliveryDate,
                Status          = (int) booking.Status, 
                Products        = await GetExistingNotLinkedProductsById(booking.Products)
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

        public async Task<BookingOutbound> UpdateItemById(Guid id, BookingInbound bookingToUpdate)
        {
            var existingBooking = await _bookingRepository.GetById(id);

            if (existingBooking == null)
            {
                throw new Exception(message: $"Booking Not Found by id: '{id}'");
            }

            var linkedProducts = existingBooking.Products.ToList();
            if (bookingToUpdate.Products?.Count() > 0)
            {
                var existingProducts = await GetExistingNotLinkedProductsById(bookingToUpdate.Products);
                existingProducts.ForEach(e => e.BookingDtoId = existingBooking.Id);
                linkedProducts.AddRange(existingProducts);
            }

            var bookingDto = new BookingDto
            {
                Id              = existingBooking.Id,
                Name            = bookingToUpdate.Name,
                CustomerEmail   = bookingToUpdate.CustomerEmail,
                CreatedDate     = existingBooking.CreatedDate,
                DeliveryAddress = bookingToUpdate.DeliveryAddress,
                DeliveryDate    = bookingToUpdate.DeliveryDate,
                Status          = existingBooking.Status,
                Products        = linkedProducts
            };

            var dbItem = await _bookingRepository.Update(bookingDto);
            return _mapper.Map<BookingOutbound>(dbItem);
        }

        public async Task<BookingOutbound> UpdateItemStatusById(Guid id, BookingStatus status)
        {
            var dbItem = await _bookingRepository.UpdateStatusById(id, (int)status);
            return _mapper.Map<BookingOutbound>(dbItem);
        }

        private async Task<List<ProductDto>> GetExistingNotLinkedProductsById(IEnumerable<Guid> ids)
        {
            if (ids.Count() == 0)
                throw new ArgumentNullException($"Booking should have at least one product");

            var products = new List<ProductDto>();

            foreach (var id in ids)
            {
                var product = await _productRepository.GetById(id);
                if (product == null)
                {
                    throw new Exception(message: $"Product Not Found by id: '{id}'");
                }
                if (product.BookingDtoId != null)
                {
                    throw new Exception(message: $"Product with id '{product.Id}' already linked to booking with id '{product.BookingDtoId}'");
                }
                products.Add(product);
            }

            return products;
        }
    }
}
