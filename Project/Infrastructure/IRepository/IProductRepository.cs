using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface IProductRepository 
    {
        Task<List<Product>> GetFeaturedProductsAsync();
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<Product> GetProductWithDetailsAsync(int id);

        Task AddAsync(Product product);
    }
}
