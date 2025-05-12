using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.IRepository;

namespace Core.Services
{
    public class OrderService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;

        public OrderService(IUnitOfWork unitOfWork, CartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public async Task<Order> CreateOrderAsync(int customerId, string shippingAddress, string paymentMethod)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

            if (!cart.CartItems.Any())
                throw new Exception("Cart is empty");

            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = await GenerateOrderNumberAsync(),
                TotalAmount = cart.TotalAmount,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            await _cartService.TransferCartToOrderAsync(customerId, order);

            await _unitOfWork._order.Add(order);
            await _unitOfWork.CompleteAsync();

            return order;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
            => await _unitOfWork._order.GetById(orderId);

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId)
            => await _unitOfWork._order.Search(o => o.CustomerId == customerId);

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork._order.GetById(orderId);
            if (order == null) return;

            order.Status = status;
            _unitOfWork._order.Update(order);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ProcessPaymentAsync(int orderId, Payment payment)
        {
            var order = await _unitOfWork._order.GetById(orderId);
            if (order == null) return;

            await _unitOfWork._payment.Add(payment);
            await _unitOfWork.CompleteAsync();

            order.PaymentId = payment.Id;
            order.PaymentStatus = payment.Status;

            if (payment.Status == "Completed")
            {
                order.Status = "Processing";
            }

            _unitOfWork._order.Update(order);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var prefix = "TX";
            var random = new Random();
            var number = random.Next(100000, 999999);
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{number}";
        }
    }
}
