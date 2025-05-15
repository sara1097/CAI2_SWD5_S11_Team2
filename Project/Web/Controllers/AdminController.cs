using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly OrderService _orderService;
        private readonly ReviewService _reviewService;

        public AdminController(OrderService orderService, ReviewService reviewService)
        {
            _orderService = orderService;
            _reviewService = reviewService;
        }

        public IActionResult Products()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Orders(string filter = "all")
        {
            ViewBag.CurrentFilter = filter;

            if (Enum.TryParse<OrderStatus>(filter, true, out var status))
            {
                return View(await _orderService.FilterByStatusAsync(status));
            }

            return View(await _orderService.GetAllOrdersAsync());
        }

        public async Task<IActionResult> Reviews(string filter = "all")
        {
            ViewBag.CurrentFilter = filter;

            if (filter.ToLower() == "pending")
            {
                return View(await _reviewService.GetPendingReviewsAsync());
            }
            else
            {
                return View(await _reviewService.GetAllReviewsAsync());
            }
        }




    }
}
