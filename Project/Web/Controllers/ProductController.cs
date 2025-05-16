using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrasitructure.Helpers;
using Domain.ViewModel;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly IWebHostEnvironment _webHost;
        private readonly string[] _allowedExtensions = { ".jpg", ".png", ".jpeg" };
        private readonly IUnitOfWork _unitOfWork;

        private readonly CategoryService _categoryService;

        public ProductController(ProductService productService, IWebHostEnvironment webHost, IUnitOfWork unitOfWork, CategoryService categoryService)
        {
            _productService = productService;
            _webHost = webHost;
            _unitOfWork = unitOfWork;
            _categoryService = categoryService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var productViewModel = new ProductViewModel
            {
                Categories = await _categoryService.GetCategoriesWithSelectListItem()

            };
            return View(productViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                productViewModel.Categories = await _categoryService.GetCategoriesWithSelectListItem();
                return View(productViewModel);
            }

            var fileExtension = Path.GetExtension(productViewModel.File.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("File", "File extension is not allowed");
                return View(productViewModel);
            }

            productViewModel.ImageUrl = await UploadFiles.UploadFile(productViewModel.File, "Products", _webHost.WebRootPath);

            var product = new Product
            {
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Price = productViewModel.Price,
                Discount = productViewModel.Discount,
                StockQuantity = productViewModel.StockQuantity,
                CategoryId = productViewModel.CategoryId,

                ImageUrl = productViewModel.ImageUrl

            };

            await _productService.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            var vm = new ProductViewModel
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = (decimal)product.Discount,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,

                ImageUrl = product.ImageUrl,
                Categories = (List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>)await _categoryService.GetCategoriesWithSelectListItem()

            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = (List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>)await _categoryService.GetCategoriesWithSelectListItem();
                return View(vm);
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.Discount = vm.Discount;
            product.StockQuantity = vm.StockQuantity;
            product.CategoryId = vm.CategoryId;


            if (vm.File != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.File.FileName);
                var filePath = Path.Combine(_webHost.WebRootPath, "Images", "Products", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.File.CopyToAsync(stream);

                product.ImageUrl = "/Images/Products/" + fileName;
            }

            await _productService.UpdateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }



        // POST: Product/ApplyDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(int id, decimal discountPercentage)
        {
            // Call the service method to apply discount
            await _productService.ApplyDiscountAsync(id, discountPercentage);
            return RedirectToAction(nameof(Index));
        }


    }
}