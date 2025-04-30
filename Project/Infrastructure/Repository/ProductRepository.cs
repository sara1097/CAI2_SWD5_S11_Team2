using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetFeaturedProductsAsync()
        {
            var featuredProducts = await _context.Products
                .Where(p => p.IsFeatured)
                .Include(p => p.Category)  // Eager load Category
                .ToListAsync();

            return featuredProducts;
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)  // Eager load Category
                .ToListAsync();

            return products;
        }

        public async Task<Product> GetProductWithDetailsAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Include(p => p.Category)  // Eager load Category
                .Include(p => p.Reviews)  // Eager load Reviews
                    .ThenInclude(r => r.Customer)  // Eager load Customer in Reviews
                        .ThenInclude(c => c.User)  // Eager load User in Customer
                .FirstOrDefaultAsync();

            return product;
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            var products = await _context.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .Include(p => p.Category)  // Eager load Category
                .ToListAsync();

            return products;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var allProducts = await _context.Products
                .Include(p => p.Category)  // Eager load Category
                .ToListAsync();

            return allProducts;
        }
    }
}
