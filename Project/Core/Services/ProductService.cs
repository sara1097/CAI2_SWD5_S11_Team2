using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.IRepository;
using Microsoft.Extensions.Hosting;

namespace Core.Services
{
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        //private readonly Action<string> _logAction;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            //_logAction = message => Console.WriteLine(message);
        }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _unitOfWork._product.GetAll();
        }
        public async Task<List<Product>> GetAllProductsAsync(Expression<Func<Product, bool>> criteria = null, // where
            Expression<Func<Product, object>>[] includes = null)
        {
            return await _unitOfWork._product.GetAll(criteria, includes);
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _unitOfWork._product.GetById(id);
        }
        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId) 
            => await _unitOfWork._product.Search(p => p.CategoryId == categoryId);

        public async Task<List<Product>> GetFeaturedProductsAsync() 
            => await _unitOfWork._product.Search(p => p.IsFeatured);
        public async Task AddProductAsync(Product post)
        {
            await _unitOfWork._product.Add(post);
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateProductAsync(Product post)
        {
             _unitOfWork._product.Update(post);
            await _unitOfWork.CompleteAsync();
        }
        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork._product.GetById(id);
            if (product != null)
            {
                _unitOfWork._product.Delete(product);
                await _unitOfWork.CompleteAsync();

            }

        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        => await _unitOfWork._product.Search(p =>
            p.Name.Contains(searchTerm) ||
            p.Description.Contains(searchTerm));

        public async Task AddToFavoritesAsync(int customerId, int productId)
        {
            var existing = await _unitOfWork._customerFavoriteProducts
                .Search(cf => cf.CustomerId == customerId && cf.ProductId == productId);

            if (!existing.Any())
            {
                await _unitOfWork._customerFavoriteProducts.Add(new CustomerFavoriteProduct
                {
                    CustomerId = customerId,
                    ProductId = productId
                } );
                await _unitOfWork.CompleteAsync();
            }
        }
        public async Task RemoveFromFavoritesAsync(int customerId, int productId)
        {
            var favorites = await _unitOfWork._customerFavoriteProducts
                .Search(cf => cf.CustomerId == customerId && cf.ProductId == productId);

            foreach (var favorite in favorites)
            {
                _unitOfWork._customerFavoriteProducts.Delete(favorite);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> IsProductInFavoritesAsync(int customerId, int productId)
        {
            var favorites = await _unitOfWork._customerFavoriteProducts
                .Search(cf => cf.CustomerId == customerId && cf.ProductId == productId);

            return favorites.Any();
        }
    }
}
