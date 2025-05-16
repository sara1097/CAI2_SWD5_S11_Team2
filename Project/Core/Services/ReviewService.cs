using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class ReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            // Use the existing BaseRepository to get reviews with includes
            Expression<Func<Review, object>>[] includes = new Expression<Func<Review, object>>[]
            {
                r => r.Product,
                r => r.Customer.User
            };

            var reviews = await _unitOfWork._review.GetAll(includes: includes);
            return reviews.OrderByDescending(r => r.ReviewDate).ToList();
        }

        public async Task<List<Review>> GetPendingReviewsAsync()
        {
            Expression<Func<Review, object>>[] includes = new Expression<Func<Review, object>>[]
            {
                r => r.Product,
                r => r.Customer.User
            };

            var reviews = await _unitOfWork._review.GetAll(
                criteria: r => r.Status == ReviewStatus.Pending,
                includes: includes
            );

            return reviews.OrderByDescending(r => r.ReviewDate).ToList();
        }

        public async Task<Review> GetReviewByIdAsync(int id)
        {
            // Get review by ID with includes
            Expression<Func<Review, object>>[] includes = new Expression<Func<Review, object>>[]
            {
                r => r.Product.Category,
                r => r.Customer.User
            };

            var reviews = await _unitOfWork._review.GetAll(
                criteria: r => r.Id == id,
                includes: includes
            );

            return reviews.FirstOrDefault();
        }


        public async Task<bool> ApproveReviewAsync(int id)
        {
            var review = await _unitOfWork._review.GetById(id);
            if (review == null)
                return false;

            review.Status = ReviewStatus.Approved;
            _unitOfWork._review.Update(review);
            await _unitOfWork.CompleteAsync();
            return true;
        }


        public async Task<bool> RejectReviewAsync(int id)
        {
            var review = await _unitOfWork._review.GetById(id);
            if (review == null)
                return false;

            review.Status = ReviewStatus.Rejected;
            _unitOfWork._review.Update(review);
            await _unitOfWork.CompleteAsync();
            return true;
        }


        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _unitOfWork._review.GetById(id);
            if (review == null)
                return false;

            _unitOfWork._review.Delete(review);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task AddReviewAsync(string comment, int rating, int customerId, int productId)
        {
            var review = new Review
            {
                ProductId = productId,
                CustomerId = customerId,
                Rating = rating,
                Comment = comment,
                Status = ReviewStatus.Pending,
                ReviewDate = DateTime.Now
            };

            await _unitOfWork._review.Add(review);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<int?> GetCustomerIdByUserIdAsync(string userId)
        {
            var customer = await _unitOfWork._customer.GetFirstOrDefault(c => c.UserId == userId);
            return customer?.Id;
        }


    }
}