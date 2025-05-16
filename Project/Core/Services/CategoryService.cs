using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Models;
using Infrastructure.IRepository;
using System.Linq.Expressions;
using System.Threading.Tasks;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core.Services
{
    public class CategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly ProductService _productService;

        public CategoryService(IUnitOfWork unitOfWork)//, ProductService productService
        {
            _unitOfWork = unitOfWork;
            //_productService = productService;
        }
        public async Task<List<Category>> GetAllCategoriesWithProductsAsync()
        {
            return await _unitOfWork._category.GetAll();
        }

        //public async void CreateCategory(Category category)
        //{
        //     _unitOfWork._category.Add(category);
        //}

        public async Task AddCategoryAsync(Category category)
        {
            await _unitOfWork._categoryRepo.AddAsync(category); // ← use repo, not IBaseRepository
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork._categoryRepo.GetByIdAsync(id);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await _unitOfWork._categoryRepo.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork._categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                await _unitOfWork._categoryRepo.DeleteAsync(category);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<List<SelectListItem>> GetCategoriesWithSelectListItem()
        {
            var categories = await _unitOfWork._category.GetAll();
            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
    }
}