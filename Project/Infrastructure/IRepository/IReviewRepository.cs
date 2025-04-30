using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure.IRepository
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetReviewsByProductIdAsync(int productId);
        Task AddReviewAsync(Review review);
    }
}
