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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category> GetCategoryWithProductsAsync(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (category != null)
            {
                _context.Entry(category).Collection(c => c.Products).Load();  // collection
            }

            return category;
        }

        public async Task<List<Category>> GetAllCategoriesWithProductsAsync()
        {
            var categories = await _context.Categories.ToListAsync();

            foreach (var category in categories)
            {
                _context.Entry(category).Collection(c => c.Products).Load(); // Explicitly load products
            }

            return categories;
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
        }
    }
}