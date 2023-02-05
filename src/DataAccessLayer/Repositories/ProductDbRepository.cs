using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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
            var productEntity = await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return productEntity.Entity;
        }

        public IQueryable<ProductDto> GetAll()
        {
           return _dbContext.Products.AsNoTracking();
        }

        public async Task<ProductDto> GetById(Guid id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<ProductDto> UpdateById(Guid id, ProductDto product)
        {
            product.Id = id;
            _dbContext.Attach(product);
            _dbContext.Entry(product).State = EntityState.Modified;
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return product;
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
