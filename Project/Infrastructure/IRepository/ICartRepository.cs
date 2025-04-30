using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerAsync(int customerId);
        Task<Cart> GetCartWithItemsAsync(int id);
        Task AddItemToCartAsync(int cartId, int productId, int quantity, string color , string size);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveItemFromCartAsync(int cartItemId);
        Task ClearCartAsync(int cartId);
    }
}
