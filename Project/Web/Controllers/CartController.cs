using System;
using System.Threading.Tasks;
using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.IRepository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
//using Stripe.BillingPortal;
using Stripe.Checkout;
using Stripe.Climate;


namespace Web.Controllers
{
  //  [Area("Shop")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly Core.Services.ProductService _productService;
        private readonly Core.Services.OrderService _orderService;
        private readonly UserManager<User> _userManager;
        private readonly ICustomerRepository _customerRepository;
       public CartController(
            CartService cartService,
            Core.Services.ProductService productService,
            Core.Services.OrderService orderService,
            UserManager<User> userManager,
            ICustomerRepository customerRepository)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
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
                    return RedirectToAction("AllP", "Customer");
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var customer = await _customerRepository.GetByUserIdAsync(userId);

                // Get product to check stock
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("AllP", "Customer");
                }

                if (quantity > product.StockQuantity)
                {
                    TempData["ErrorMessage"] = $"Only {product.StockQuantity} items available in stock";
                    //return RedirectToAction("Details", "Customer", new { id = productId });
                    return RedirectToAction("AllP", "Customer");
                }

                // Calculate discounted price if available
                decimal unitPrice = product.Price;
                if (product.Discount.HasValue && product.Discount.Value > 0)
                {
                    unitPrice = product.Price * (1 - product.Discount.Value);
                }

                await _cartService.AddToCartAsync(customer.Id, productId, quantity, unitPrice);

                // Redirect directly to cart page
                TempData["SuccessMessage"] = $"{product.Name} added to cart successfully";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                //return RedirectToAction("Details", "Customer", new { id = productId });
                return RedirectToAction("AllP", "Customer");
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

        [HttpGet]
        public async Task<IActionResult> Checkout()

        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            var cart = await _cartService.GetCartByCustomerIdAsync(customer.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string paymentMethod, string shippingAddress)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var customer = await _customerRepository.GetByUserIdAsync(userId);

                if (paymentMethod == "Credit")
                {
                    // Store shipping address temporarily for payment process
                    TempData["ShippingAddress"] = shippingAddress;
                    return await CheckOutPayment();
                }
                else if (paymentMethod == "Cash")
                {
                    var order = await _orderService.CreateOrderFromCartAsync(
                        customer.Id,
                        shippingAddress,
                        Domain.Models.PaymentMethod.CashOnDelivery);

                    await ClearCart();
                    return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
                }

                var cart = await _cartService.GetCartByCustomerIdAsync(customer.Id);
                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout");
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOutPayment()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return NotFound();

                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer == null) return NotFound();

                var cart = await _cartService.GetCartByCustomerIdAsync(customer.Id);
                if (cart == null || !cart.CartItems.Any()) return NotFound();

                var shippingAddress = TempData["ShippingAddress"]?.ToString();
                if (string.IsNullOrEmpty(shippingAddress))
                {
                    TempData["ErrorMessage"] = "Shipping address is missing";
                    return RedirectToAction("Checkout");
                }

                var domain = "http://localhost:5105/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Checkout/StripeSuccess?customerId={customer.Id}&shippingAddress={Uri.EscapeDataString(shippingAddress)}",
                    CancelUrl = domain + "Checkout/Checkout",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    Metadata = new Dictionary<string, string>
            {
                { "customerId", customer.Id.ToString() },
                { "shippingAddress", shippingAddress }
            }
                };

                foreach (var item in cart.CartItems)
                {
                    var sessionListItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.UnitPrice * 100), // Convert to cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            },
                        },
                        Quantity = item.Quantity,
                    };
                    options.LineItems.Add(sessionListItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout");
            }
        }

        [HttpGet]
        public async Task<IActionResult> StripeSuccess(string customerId, string shippingAddress)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer == null) return NotFound();

                var order = await _orderService.CreateOrderFromCartAsync(
                    customer.Id,
                    shippingAddress,
                    Domain.Models.PaymentMethod.Stripe);

                ClearCart();
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout");
            }
        }
    }
}
