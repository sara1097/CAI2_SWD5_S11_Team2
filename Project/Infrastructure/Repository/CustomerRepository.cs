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
