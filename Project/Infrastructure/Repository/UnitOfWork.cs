using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;


        public IBaseRepository<Product> _product { get; private set; }
        public IBaseRepository<Category> _category { get; private set; }
        public IBaseRepository<Order> _order { get; }
        public IBaseRepository<OrderItem> _orderItem { get; }
        public IBaseRepository<Cart> _cart { get; }
        public IBaseRepository<CartItem> _cartItem { get; }
        public IBaseRepository<Payment> _payment { get; }
        public IBaseRepository<Review> _review { get; }
        public IBaseRepository<Customer> _customer { get; }
        public IBaseRepository<Admin> _admin { get; }
        public IBaseRepository<CustomerFavoriteProduct> _customerFavoriteProducts { get; }

        // Initialize the ProductRepository
        public IProductRepository _productRepo { get; private set; }
        public ICategoryRepository _categoryRepo { get; private set; }


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            _product = new BaseRepository<Product>(_context);
            _category = new BaseRepository<Category>(_context);
            _order = new BaseRepository<Order>(_context);
            _orderItem = new BaseRepository<OrderItem>(_context);
            _cart = new BaseRepository<Cart>(_context);
            _cartItem = new BaseRepository<CartItem>(_context);
            _payment = new BaseRepository<Payment>(_context);
            _review = new BaseRepository<Review>(_context);
            _customer = new BaseRepository<Customer>(_context);
            _admin = new BaseRepository<Admin>(_context);
            _customerFavoriteProducts = new BaseRepository<CustomerFavoriteProduct>(_context);
            
            // Initialize the ProductRepository
            _productRepo = new ProductRepository(_context);
            _categoryRepo = new CategoryRepository(_context);
        }


        public int Complete()
        {
            return _context.SaveChanges();
        }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction is not null)
                    await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction is not null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

            }
        }
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction is not null)
                    await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction is not null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
        public void Dispose()
        {
            _context.Dispose();
        }

    }

}
