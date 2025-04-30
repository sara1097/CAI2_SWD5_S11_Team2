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
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                _context.Entry(order).Collection(o => o.OrderItems).Load(); // collection
                foreach (var item in order.OrderItems)
                {
                    _context.Entry(item).Reference(oi => oi.Product).Load(); // object
                }

                _context.Entry(order).Reference(o => o.Payment).Load(); // object
            }

            return orders;
        }

        public async Task<Order> GetOrderWithItemsAsync(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                _context.Entry(order).Reference(o => o.Customer).Load(); // object
                _context.Entry(order.Customer).Reference(c => c.User).Load(); // object

                _context.Entry(order).Collection(o => o.OrderItems).Load(); // collection
                foreach (var item in order.OrderItems)
                {
                    _context.Entry(item).Reference(oi => oi.Product).Load(); // object
                }

                _context.Entry(order).Reference(o => o.Payment).Load(); // object
            }

            return order;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "ORD-" + dateStr + "-";

            var latestOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (latestOrder != null)
            {
                string seqStr = latestOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(seqStr, out int latestSeq))
                {
                    sequence = latestSeq + 1;
                }
            }

            return prefix + sequence.ToString("D3");
        }
    }
}
