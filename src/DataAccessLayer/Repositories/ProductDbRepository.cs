using DataAccessLayer.DTO;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
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

        public async Task<ProductDto> Add(ProductDto product, CancellationToken cancellationToken = default)
        {
            //product.Id = Guid.NewGuid();
            var productEntity = await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync();
            return productEntity.Entity;
        }

        public async Task<(IQueryable<ProductDto> FilteredItems, int TotalCount)> GetAll(ItemsRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Products.AsNoTracking();
            if (!string.IsNullOrEmpty(request.ItemName))
            {
                query = query.Where(item => item.Name.Contains(request.ItemName));
            }
            // bottleneck ??
            int totalCount = await query.CountAsync(cancellationToken);
            query = query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
            return (query, totalCount);
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
            await _dbContext.SaveChangesAsync();
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
