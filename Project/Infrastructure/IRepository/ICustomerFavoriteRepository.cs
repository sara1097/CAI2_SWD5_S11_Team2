using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface ICustomerFavoriteRepository
    {
        Task<List<Product>> GetFavoriteProductsAsync(int customerId);
        Task AddToFavoritesAsync(int customerId, int productId);
        Task RemoveFromFavoritesAsync(int customerId, int productId);
    }
}
