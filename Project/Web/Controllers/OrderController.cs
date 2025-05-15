using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }


        // GET: Order/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id, bool partial = false)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);
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