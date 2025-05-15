using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;
        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }


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
