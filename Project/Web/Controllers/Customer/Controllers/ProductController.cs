using Core.Services;
using Domain.Models;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Customer.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly UserManager<User> _userManager;
        //private readonly IUnitOfWork _unitOfWork;

        public ProductController(
            ProductService productService,
            CartService cartService,
            UserManager<User> userManager)
        {
            _productService = productService;
            _cartService = cartService;
            _userManager = userManager;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string searchTerm)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = await _productService.SearchProductsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId.Value;
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            return View(products);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddToCart(int productId, int quantity,
        //    string color, string size)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var customer = await _unitOfWork.Customers.FindAsync(c => c.UserId == user.Id);
        //    var customerId = customer.FirstOrDefault()?.Id;

        //    if (customerId == null) return RedirectToAction("Login", "Account");

        //    try
        //    {
        //        await _cartService.AddToCartAsync(customerId.Value, productId, quantity, color, size);
        //        TempData["Success"] = "Product added to cart successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //    }

        //    return RedirectToAction("Details", new { id = productId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddToFavorites(int productId)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var customer = await _unitOfWork.Customers.FindAsync(c => c.UserId == user.Id);
        //    var customerId = customer.FirstOrDefault()?.Id;

        //    if (customerId == null) return RedirectToAction("Login", "Account");

        //    await _productService.AddToFavoritesAsync(customerId.Value, productId);
        //    TempData["Success"] = "Product added to favorites";

        //    return RedirectToAction("Details", new { id = productId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> RemoveFromFavorites(int productId)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var customer = await _unitOfWork.Customers.FindAsync(c => c.UserId == user.Id);
        //    var customerId = customer.FirstOrDefault()?.Id;

        //    if (customerId == null) return RedirectToAction("Login", "Account");

        //    await _productService.RemoveFromFavoritesAsync(customerId.Value, productId);
        //    TempData["Success"] = "Product removed from favorites";

        //    return RedirectToAction("Details", new { id = productId });
        //}

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
    }
}
