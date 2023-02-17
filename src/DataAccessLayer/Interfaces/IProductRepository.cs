using DataAccessLayer.DTO;
using DataAccessLayer.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IProductRepository
    {
        /// <summary>
        /// Add Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns>
        /// Newly created Product
        /// </returns>
        Task<ProductDto> Add(ProductDto product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all Products by predicate Items Request
        /// </summary>
        /// <returns>
        /// Products collection or empty collection
        /// </returns>
        Task<(IQueryable<ProductDto> FilteredItems, int TotalCount)> GetAll(ItemsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Product with pointed ID or null if Product was not found by ID
        /// </returns>
        Task<ProductDto> GetById(Guid id);

        /// <summary>
        /// Update Product by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns>
        /// Updated Product or null if Product was not found by id
        /// </returns>
        Task<ProductDto> UpdateById(Guid Guid, ProductDto product);

        /// <summary>
        /// Delete Product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// 1 if Product was deleted successfully or 0 if Product was not found by id or not deleted
        /// </returns>
        Task<int> RemoveItemById(Guid id);
    }
}
