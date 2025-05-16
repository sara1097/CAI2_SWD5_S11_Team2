using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Services;
using Domain.Models;
using Infrastructure.IRepository;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        //private readonly CartService _cartService;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            //_cartService = cartService;
        }


        public async Task<List<Order>> GetAllOrdersAsync()
        {
            Expression<Func<Order, object>>[] includes = {
                o => o.Customer,
                o => o.Payment
            };

            var orders = await _unitOfWork._order.GetAll(includes: includes);
            return orders.OrderByDescending(o => o.OrderDate).ToList();
        }

        public Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return _orderRepository.GetOrdersByCustomerAsync(userId);
        }



        public async Task<List<Order>> FilterByStatusAsync(OrderStatus status)
        {
            Expression<Func<Order, object>>[] includes = new Expression<Func<Order, object>>[]
            {
                o => o.Customer,
                o => o.Payment
            };


            var orders = await _unitOfWork._order.GetAll(
                criteria: r => r.Status == status,
                includes: includes
            );

            return orders.OrderByDescending(r => r.OrderDate).ToList();
        }

        public async Task<Order> GetOrderWithDetailsAsync(int id)
        {
            Expression<Func<Order, bool>> criteria = o => o.Id == id;
            Expression<Func<Order, object>>[] includes = {
                o => o.OrderItems,
                o => o.Customer,
                o => o.Payment
            };

            // Get order with all its related entities
            var orders = await _unitOfWork._order.GetAll(criteria, includes);
            var order = orders.FirstOrDefault();

            if (order != null && order.OrderItems != null)
            {
                // For each order item, load the associated product
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork._product.GetById(item.ProductId);
                    if (product != null)
                    {
                        item.Product = product;
                    }
                }
            }

            return order;
        }

        public async Task<bool> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            try
            {
                var order = await _unitOfWork._order.GetById(orderId);
                if (order == null)
                    return false;

                // Update order status
                order.Status = newStatus;

                // Update shipped/delivered dates based on status
                if (newStatus == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.UtcNow;
                }
                else if (newStatus == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                {
                    order.DeliveredDate = DateTime.UtcNow;
                }

                _unitOfWork._order.Update(order);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CancelOrder(int orderId)
        {
            try
            {
                var order = await _unitOfWork._order.GetById(orderId);
                if (order == null)
                    return false;

                // Check if order can be cancelled (only pending or processing orders)
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                {
                    return false;
                }

                order.Status = OrderStatus.Cancelled;
                _unitOfWork._order.Update(order);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Order> CreateOrderFromCartAsync(int customerId, string shippingAddress, PaymentMethod paymentMethod)
        {
            // Get the customer's cart
            var cart = await _unitOfWork._cartRepo.GetCartWithItemsAsync(customerId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty or not found.");

            // Create the order
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = await GenerateOrderNumberAsync(),
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.TotalAmount,
                OrderItems = new List<OrderItem>()
            };

            // Transfer cart items to order items
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _unitOfWork._product.GetById(cartItem.ProductId);
                if (product == null)
                    throw new Exception($"Product {cartItem.ProductId} not found");

                if (product.StockQuantity < cartItem.Quantity)
                    throw new Exception($"Not enough stock for product: {product.Name}");

                // Update product stock
                product.StockQuantity -= cartItem.Quantity;
                _unitOfWork._product.Update(product);

                // Add order item
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    SubtotalAmount = cartItem.SubtotalAmount
                });
            }

            // Save the order
            await _unitOfWork._order.Add(order);

            // Clear the cart
            await _unitOfWork._cartRepo.ClearCartAsync(cart.Id);

            // Complete the transaction
            await _unitOfWork.CompleteAsync();

            return order;
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var prefix = "ORD";
            var random = new Random();
            var number = random.Next(100000, 999999);
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{number}";
        }

        //public async Task<Order> CreateOrderFromCart(int cartId, string userId)
        //{
        //    var cart = await _unitOfWork._cart.GetById(cartId);

        //    var order = new Order
        //    {
        //        UserId = userId,
        //        OrderDate = DateTime.UtcNow,
        //        Status = OrderStatus.Pending,
        //        OrderItems = cart.Items.Select(ci => new OrderItem
        //        {
        //            ProductId = ci.ProductId,
        //            Quantity = ci.Quantity,
        //            UnitPrice = ci.Product.Price, // Get current price
        //            Color = ci.Color,
        //            Size = ci.Size,
        //            // CartItemId = ci.Id // Only include if using Option 2
        //        }).ToList()
        //    };

        //    // Calculate order total
        //    foreach (var item in order.OrderItems)
        //    {
        //        item.CalculateSubtotal();
        //    }

        //    await _orderRepository.AddAsync(order);
        //    await _cartRepository.ClearCart(cartId); // Remove cart items

        //    return order;
        //}
    }
}


//public async Task<Order> CreateOrderAsync(int customerId, string shippingAddress, string paymentMethod)
//{
//    var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

//    if (!cart.CartItems.Any())
//        throw new Exception("Cart is empty");

//    if (Enum.TryParse<PaymentMethod>(paymentMethod, out var parsedPaymentMethod))
//    {
//        var order = new Order
//        {
//            CustomerId = customerId,
//            OrderNumber = await GenerateOrderNumberAsync(),
//            TotalAmount = cart.TotalAmount,
//            Status = OrderStatus.Pending,
//            PaymentStatus = PaymentStatus.Pending,
//            ShippingAddress = shippingAddress,
//            PaymentMethod = parsedPaymentMethod,
//            OrderDate = DateTime.UtcNow,
//            OrderItems = new List<OrderItem>()
//        };

//        await _cartService.TransferCartToOrderAsync(customerId, order);
//        await _unitOfWork._order.Add(order);
//        await _unitOfWork.CompleteAsync();
//        return order;
//    }
//    else
//    {
//        throw new ArgumentException($"Invalid PaymentMethod value: {paymentMethod}");
//    }


//}
//public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
//    => await _unitOfWork._order.Search(o => o.CustomerId == customerId);
//public async Task ProcessPaymentAsync(int orderId, Payment payment)
//{
//    var order = await _unitOfWork._order.GetById(orderId);
//    if (order == null) return;

//    await _unitOfWork._payment.Add(payment);
//    await _unitOfWork.CompleteAsync();

//    order.PaymentId = payment.Id;
//    order.PaymentStatus = payment.Status;

//    if (payment.Status == PaymentStatus.Paid)
//    {
//        order.Status = OrderStatus.Processing; // Fixed by qualifying with the type name
//    }

//    _unitOfWork._order.Update(order);
//    await _unitOfWork.CompleteAsync();
//}
//public async Task<string> GenerateOrderNumberAsync()
//{
//    var prefix = "TX";
//    var random = new Random();
//    var number = random.Next(100000, 999999);
//    return $"{prefix}-{DateTime.Now:yyyyMMdd}-{number}";
//}



//public async Task UpdateOrderStatusAsync(int orderId, string status)
//{
//    var order = await _unitOfWork._order.GetById(orderId);
//    if (order == null) return;

//    if (Enum.TryParse<OrderStatus>(status, out var parsedStatus))
//    {
//        order.Status = parsedStatus;
//        _unitOfWork._order.Update(order);
//        await _unitOfWork.CompleteAsync();
//    }
//    else
//    {
//        throw new ArgumentException($"Invalid status value: {status}");
//    }
//}

//public async Task CancelOrderAsync(int orderId)
//{
//    var order = await _unitOfWork._order.GetById(orderId);
//    if (order == null) return;
//    order.Status = OrderStatus.Cancelled;
//    _unitOfWork._order.Update(order);
//    await _unitOfWork.CompleteAsync();
//}

//public async Task<Order> GetOrderByIdAsync(int orderId)
//    => await _unitOfWork._order.GetById(orderId);

//public async Task<List<Order>> GetAllOrdersAsync()
//{
//    return await _unitOfWork._order.GetAll();
//}