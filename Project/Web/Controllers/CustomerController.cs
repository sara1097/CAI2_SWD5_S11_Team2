using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;

        public CustomerController(CategoryService categoryService, ProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.Reviews ??= new List<Review>();
            return View(product); // this will look for Views/Customer/Details.cshtml
        }

        public async Task<IActionResult> AllP()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }


        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesWithProductsAsync();
            return View(categories);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}