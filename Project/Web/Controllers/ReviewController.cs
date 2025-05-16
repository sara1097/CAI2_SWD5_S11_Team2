using Core.Services;
using Domain.Models;
using Domain.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;

namespace Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;

        private readonly UserManager<User> _userManager;
        public ReviewController(ReviewService reviewService, UserManager<User> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }



        [HttpPost]
        public async Task<IActionResult> SubmitReview(string comment, int rating, int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var customerId = await _reviewService.GetCustomerIdByUserIdAsync(user.Id);
            if (customerId == null)
                return NotFound("Customer not found.");

            await _reviewService.AddReviewAsync(comment, rating, (int)customerId, productId);
            return RedirectToAction("Details", "Customer", new { id = productId });
        }

        //


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _reviewService.ApproveReviewAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to approve review.";
                return NotFound();
            }

            TempData["SuccessMessage"] = "Review approved successfully.";
            return RedirectToAction("Reviews", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _reviewService.RejectReviewAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to reject review.";
                return NotFound();
            }

            TempData["SuccessMessage"] = "Review rejected successfully.";
            return RedirectToAction("Reviews", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to delete review.";
                return NotFound();
            }

            TempData["SuccessMessage"] = "Review deleted successfully.";
            return RedirectToAction("Reviews", "Admin");
        }
    }
}