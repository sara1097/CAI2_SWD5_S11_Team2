using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

public class CustomerFavoriteRepository : ICustomerFavoriteRepository
{
    private readonly AppDbContext _context;

    public CustomerFavoriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetFavoriteProductsAsync(int customerId)
    {
        return await _context.CustomerFavoriteProducts
            .Where(f => f.CustomerId == customerId)
            .Include(f => f.Product)
            .Select(f => f.Product)
            .ToListAsync();
    }

    public async Task AddToFavoritesAsync(int customerId, int productId)
    {
        var fav = new CustomerFavoriteProduct
        {
            CustomerId = customerId,
            ProductId = productId
        };
        _context.CustomerFavoriteProducts.Add(fav);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromFavoritesAsync(int customerId, int productId)
    {
        var fav = await _context.CustomerFavoriteProducts
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.ProductId == productId);

        if (fav != null)
        {
            _context.CustomerFavoriteProducts.Remove(fav);
            await _context.SaveChangesAsync();
        }
    }
}
