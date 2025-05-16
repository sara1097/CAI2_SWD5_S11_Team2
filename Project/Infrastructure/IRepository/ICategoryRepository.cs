using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Repository;

namespace Infrastructure.IRepository
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);

        Task<Category> GetCategoryWithProductsAsync(int id);

        Task<List<Category>> GetAllCategoriesWithProductsAsync();

        Task<Category> GetByIdAsync(int id);

        Task UpdateAsync(Category category);

        Task DeleteAsync(Category category);

        
        
    }
}