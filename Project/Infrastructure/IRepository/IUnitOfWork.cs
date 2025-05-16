using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        // genaric
        IBaseRepository<Product> _product { get; }
        IBaseRepository<Category> _category { get; }
         IBaseRepository<Order> _order { get; }
         IBaseRepository<OrderItem> _orderItem { get; }
         IBaseRepository<Cart> _cart { get; }
         IBaseRepository<CartItem> _cartItem { get; }
         IBaseRepository<Payment> _payment { get; }
         IBaseRepository<Review> _review { get; }
         IBaseRepository<Customer> _customer { get; }
         IBaseRepository<Admin> _admin { get; }
         IBaseRepository<CustomerFavoriteProduct> _customerFavoriteProducts { get; }


        // interfaces
        IProductRepository _productRepo { get; }
        ICategoryRepository _categoryRepo { get; }
        ICartRepository _cartRepo { get; }  
        int Complete();
        Task<int> CompleteAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}
