using Core.Services;
using Domain.Models;
using Infrastructure.IRepository;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Domain.ViewModel;

namespace Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _catService;
        private readonly ProductService _productService;
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public CategoryController(CategoryService categoryService, ProductService productService)
        {
            _catService = categoryService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _catService.GetAllCategoriesWithProductsAsync();  // call repo function

            return View(categories);  // Pass to view
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Model error: " + error.ErrorMessage);
                }
                return View(viewModel);
            }

            var category = new Category
            {
                Name = viewModel.Name,
                Description = viewModel.Description
            };

            await _catService.AddCategoryAsync(category);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _catService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var category = await _catService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = viewModel.Name;
                category.Description = viewModel.Description;

                await _catService.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _catService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _catService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            await _catService.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ViewProducts(int id)
        {
            // Get category using BaseRepository
            var category = await _catService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Get products using ProductService
            var products = await _productService.GetProductsByCategoryAsync(id);


            return View(products);
        }


        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction(nameof(Index));

            var results = await _catService.SearchCategoriesAsync(keyword);
            return View("Index", results); // Reuse Index view to show results
        }



    }
}