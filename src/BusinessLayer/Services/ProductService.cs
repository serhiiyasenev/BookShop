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
    public class ProductService<Inbound, Outbound>
        : IProductService<Inbound, Outbound> where Inbound : ProductInbound where Outbound : ProductOutbound
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IMapper mapper, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        public async Task<Outbound> AddItem(Inbound booking)
        {
            var dbItem = await _productRepository.Add(_mapper.Map<ProductDto>(booking));
            return _mapper.Map<Outbound>(dbItem);
        }

        public IQueryable<Outbound> GetAllItems()
        {
            var dbItems = _productRepository.GetAll();
            return _mapper.ProjectTo<Outbound>(dbItems);
        }

        public async Task<Outbound> GetItemById(Guid id)
        {
            var dbItem = await _productRepository.GetById(id);
            return _mapper.Map<Outbound>(dbItem);
        }

        public async Task<Outbound> UpdateItemById(Guid id, Inbound booking)
        {
            var dbItem = await _productRepository.UpdateById(id, _mapper.Map<ProductDto>(booking));
            return _mapper.Map<Outbound>(dbItem);
        }

        public async Task<int> RemoveItemById(Guid id)
        {
            return await _productRepository.RemoveItemById(id);
        }
    }
}