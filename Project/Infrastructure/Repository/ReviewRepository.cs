using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
    {
        return await _context.Reviews
          .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
          .Include(r => r.Customer)
          .ThenInclude(c => c.User)
          .OrderByDescending(r => r.ReviewDate)
          .ToListAsync();
    }

    public async Task AddReviewAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
    }
}