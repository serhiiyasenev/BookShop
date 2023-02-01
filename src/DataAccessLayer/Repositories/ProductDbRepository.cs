using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class ProductDbRepository : IProductRepository
    {
        private readonly EfCoreContext _dbContext;

        public ProductDbRepository(EfCoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductDto> Add(ProductDto product)
        {
            product.Id = Guid.NewGuid();
            var userEntity = await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return userEntity.Entity;
        }

        public async Task<IEnumerable<ProductDto>> GetAll()
        {
            var result = await _dbContext.Products.ToListAsync();
            return result;
        }

        public async Task<ProductDto> GetById(Guid id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<ProductDto> UpdateById(Guid id, ProductDto user)
        {
            user.Id = id;
            _dbContext.Attach(user);
            _dbContext.Entry(user).State = EntityState.Modified;
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return user;
        }

        public async Task<int> RemoveItemById(Guid id)
        {
            var product = new ProductDto { Id = id };
            _dbContext.Attach(product);
            _dbContext.Entry(product).State = EntityState.Deleted;
            try
            {
                return await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return 0;
            }
        }
    }
}
