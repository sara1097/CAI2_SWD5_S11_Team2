using System.Security.Claims;
using Core.Services;
using Domain.Models;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly UserManager<User> _userManager;
        private readonly ICustomerRepository _customerRepository;

        public OrderController(OrderService orderService, ProductService productService, UserManager<User> userManager, ICustomerRepository customerRepository)
        {
            _orderService = orderService;
            _productService = productService;
            _userManager = userManager;
            _customerRepository = customerRepository;
        }


        // GET: Order/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id, bool partial = false)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer != null)
            {
                return NotFound();
            }
            if (order == null)
            {
                return NotFound();
            }

            // If it's a partial request, return just the details without layout
            if (partial)
            {
                return PartialView(order);
            }

            return View(order);
        }

        // GET: Order/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetailsCustomer(int id, bool partial = false)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                return NotFound();
            }
            if (order == null)
            {
                return NotFound();
            }

            // If it's a partial request, return just the details without layout
            if (partial)
            {
                return PartialView(order);
            }

            return View(order);
        }

        // GET: Order/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {

                var ordersAdmin = await _orderService.GetAllOrdersAsync();
                return View(ordersAdmin);

            }
            var orders = await _orderService.GetUserOrdersAsync(customer.Id);
            return View(orders);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            if (ModelState.IsValid)
            {
                var result = await _orderService.UpdateStatus(orderId, newStatus);
                if (result)
                {
                    TempData["Success"] = "Order status updated successfully.";
                    return RedirectToAction("Orders", "Admin");
                }
                TempData["Error"] = "Failed to update order status.";
            }
            return RedirectToAction("Orders", "Admin");
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var result = await _orderService.CancelOrder(orderId);
            if (result)
            {
                TempData["Success"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel order. Only pending or processing orders can be cancelled.";
            }
            return RedirectToAction("Orders", "Admin");
        }




    }
}