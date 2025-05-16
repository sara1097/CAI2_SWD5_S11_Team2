using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Customer> GetByUserIdAsync(string userId)
        {
            // Explicitly convert to string if needed
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId.Equals(userId));

            if (customer != null)
            {
                await _context.Entry(customer)
                    .Reference(c => c.User)
                    .LoadAsync(); // Using async version
            }

            return customer;
        }
        public async Task<Cart> CreateNewCartAsync(int customerId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                ItemCount = 0,
                TotalAmount = 0,
                CartItems = new List<CartItem>()
            };

            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        //public async Task<Customer> GetByUserIdAsync(int userId)
        //{
        //    var customer = await _context.Customers
        //        .FirstOrDefaultAsync(c => c.UserId == userId);

        //    if (customer != null)
        //    {
        //        _context.Entry(customer).Reference(c => c.User).Load(); // object
        //    }

        //    return customer;
        //}

        //public async Task<Customer> GetCustomerWithOrdersAsync(int id)
        //{
        //    var customer = await _context.Customers
        //        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (customer != null)
        //    {
        //        _context.Entry(customer).Reference(c => c.User).Load();       // object
        //        _context.Entry(customer).Collection(c => c.Orders).Load();    // collection
        //    }

        //    return customer;
        //}
    }
}
