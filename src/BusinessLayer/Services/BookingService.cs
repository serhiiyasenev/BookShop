using AutoMapper;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class BookingService<Inbound, Outbound>
        : IBookingService<Inbound, Outbound> where Inbound : BookingInbound where Outbound : BookingOutbound
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

        public async Task<IEnumerable<Outbound>> GetAllItems()
        {
            var dbItems = await _bookingRepository.GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<Outbound>>(dbItems);
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
    }
}
