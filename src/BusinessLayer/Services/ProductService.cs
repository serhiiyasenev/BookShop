﻿using AutoMapper;
using BusinessLayer.Interfaces;
using BusinessLayer.Models.Inbound;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IMapper mapper, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        public async Task<ProductOutbound> AddItem(ProductInbound booking)
        {
            var dbItem = await _productRepository.Add(_mapper.Map<ProductDto>(booking));
            return _mapper.Map<ProductOutbound>(dbItem);
        }

        public async Task<(IQueryable<ProductOutbound> FilteredItems, int TotalCount)> GetAll(RequestModel request)
        {
            var dbItems = await _productRepository.GetAll(_mapper.Map<ItemsRequest>(request));
            return (_mapper.ProjectTo<ProductOutbound>(dbItems.FilteredItems), dbItems.TotalCount);
        }

        public async Task<ProductOutbound> GetItemById(Guid id)
        {
            var dbItem = await _productRepository.GetById(id);
            return _mapper.Map<ProductOutbound>(dbItem);
        }

        public async Task<ProductOutbound> UpdateItemById(Guid id, ProductInbound booking)
        {
            var dbItem = await _productRepository.UpdateById(id, _mapper.Map<ProductDto>(booking));
            return _mapper.Map<ProductOutbound>(dbItem);
        }

        public async Task<int> RemoveItemById(Guid id)
        {
            return await _productRepository.RemoveItemById(id);
        }

        public async Task<(bool, string)> SaveImage(string path, Stream image)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return (true, "Saved successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
