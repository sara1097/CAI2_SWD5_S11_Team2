using System;
using System.Threading.Tasks;
using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
namespace Web.Controllers
{
  //  [Area("Shop")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly UserManager<User> _userManager;
        private readonly ICustomerRepository _customerRepository;
       public CartController(
            CartService cartService,
            ProductService productService,
            UserManager<User> userManager,
            ICustomerRepository customerRepository)
        {
            _cartService = cartService;
            _productService = productService;
            _userManager = userManager;
            _customerRepository = customerRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            var cart = await _cartService.GetCartByCustomerIdAsync(customer.Id);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"AddToCart called with productId: {productId}, quantity: {quantity}");
                // Validate quantity
                if (quantity < 1)
                {
                    TempData["ErrorMessage"] = "Quantity must be at least 1";
                    // return RedirectToAction("Details", "Customer", new { id = productId });
                    return RedirectToAction("Index", "Cart");
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var customer = await _customerRepository.GetByUserIdAsync(userId);

                // Get product to check stock
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("Index", "Product");
                }

                if (quantity > product.StockQuantity)
                {
                    TempData["ErrorMessage"] = $"Only {product.StockQuantity} items available in stock";
                    //return RedirectToAction("Details", "Customer", new { id = productId });
                    return RedirectToAction("Index", "Cart");
                }

                await _cartService.AddToCartAsync(customer.Id, productId, quantity);

                // Redirect directly to cart page
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                //return RedirectToAction("Details", "Customer", new { id = productId });
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            try
            {
                await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            await _cartService.ClearCartAsync(customer.Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetCartSummary()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            var itemCount = await _cartService.GetCartItemCountAsync(customer.Id);
            var totalAmount = await _cartService.CalculateCartTotalAsync(customer.Id);

            return Json(new { itemCount, totalAmount });
        }
    }
}
