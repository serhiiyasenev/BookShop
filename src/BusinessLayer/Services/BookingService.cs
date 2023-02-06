using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Inbound.Booking;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class BookingService<Inbound, Outbound>
        : IBookingService<Inbound, Outbound> where Inbound : BookingInboundWithProducts where Outbound : BookingOutbound
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public BookingService(IMapper mapper, IBookingRepository bookingRepository)
        {
            _mapper = mapper;
            _bookingRepository = bookingRepository;
        }

        public async Task<Outbound> AddItem(Inbound booking)
        {
            var dbItem = await _bookingRepository.Add(_mapper.Map<BookingDto>(booking));
            return _mapper.Map<Outbound>(dbItem);
        }

        public async Task<Outbound> AddItemWithExistingProducts(Inbound booking, IEnumerable<Guid> ids)
        {
            var dbItem = await _bookingRepository.AddWithExistingProducts(_mapper.Map<BookingDto>(booking), ids);
            return _mapper.Map<Outbound>(dbItem);
        }

        public IQueryable<Outbound> GetAllItems()
        {
            var dbItems = _bookingRepository.GetAll();
            return _mapper.ProjectTo<Outbound>(dbItems);
        }

        public async Task<Outbound> GetItemById(Guid id)
        {
            var dbItem = await _bookingRepository.GetById(id);
            return _mapper.Map<Outbound>(dbItem);
        }

        public async Task<Outbound> UpdateItemById(Guid id, Inbound booking)
        {
            var dbItem = await _bookingRepository.UpdateById(id, _mapper.Map<BookingDto>(booking));
            return _mapper.Map<Outbound>(dbItem);
        }

        public async Task<Outbound> UpdateItemStatusById(Guid id, BookingStatus status)
        {
            var test = (int)status;
            var dbItem = await _bookingRepository.UpdateStatusById(id, (int)status);
            return _mapper.Map<Outbound>(dbItem);
        }
    }
}
