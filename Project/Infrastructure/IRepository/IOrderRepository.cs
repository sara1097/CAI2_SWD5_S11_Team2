using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface IOrderRepository 
    {
        Task<List<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order> GetOrderWithItemsAsync(int id);
        Task<string> GenerateOrderNumberAsync();
    }
}
