using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface ICustomerRepository 
    {
        Task<Customer> GetByUserIdAsync(string userId);
        //Task<Customer> GetCustomerWithOrdersAsync(int id);
    }
}
