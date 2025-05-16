using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.IRepository;

namespace Core.Services
{
    public class CartService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductService _productService;

        public CartService(IUnitOfWork unitOfWork, ProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
        }

        public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
        {
            var carts = await _unitOfWork._cart.Search(c => c.CustomerId == customerId);
            return carts.FirstOrDefault() ?? await CreateNewCartAsync(customerId);
        }

        private async Task<Cart> CreateNewCartAsync(int customerId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                ItemCount = 0,
                TotalAmount = 0,
                CartItems = new List<CartItem>()
            };

            await _unitOfWork._cart.Add(cart);
            await _unitOfWork.CompleteAsync();
            return cart;
        }

        public async Task AddToCartAsync(int customerId, int productId, int quantity, string color, string size)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null || product.StockQuantity < quantity)
                throw new Exception("Product not available in requested quantity");

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == productId );

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.SubtotalAmount = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    SubtotalAmount = product.Price * quantity,
                    IsInStock = product.StockQuantity >= quantity
                };

                cart.CartItems.Add(cartItem);
            }

            await UpdateCartTotalsAsync(cart);
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _unitOfWork._cartItem.GetById(cartItemId);
            if (cartItem == null) return;

            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            if (product.StockQuantity < quantity)
                throw new Exception("Not enough stock available");

            cartItem.Quantity = quantity;
            cartItem.SubtotalAmount = quantity * cartItem.UnitPrice;
            cartItem.IsInStock = product.StockQuantity >= quantity;

            _unitOfWork._cartItem.Update(cartItem);

            var cart = await _unitOfWork._cart.GetById(cartItem.CartId);
            await UpdateCartTotalsAsync(cart);
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _unitOfWork._cartItem.GetById(cartItemId);
            if (cartItem == null) return;

            _unitOfWork._cartItem.Delete(cartItem);

            var cart = await _unitOfWork._cart.GetById(cartItem.CartId);
            await UpdateCartTotalsAsync(cart);
        }

        public async Task ClearCartAsync(int customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);

            foreach (var item in cart.CartItems.ToList())
            {
                _unitOfWork._cartItem.Delete(item);
            }

            await UpdateCartTotalsAsync(cart);
        }

        public async Task<decimal> CalculateCartTotalAsync(int customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            return cart.CartItems.Sum(ci => ci.SubtotalAmount);
        }

        public async Task<int> GetCartItemCountAsync(int customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            return cart.CartItems.Sum(ci => ci.Quantity);
        }

        public async Task TransferCartToOrderAsync(int customerId, Order order)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);

            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productService.GetProductByIdAsync(cartItem.ProductId);

                if (product.StockQuantity < cartItem.Quantity)
                    throw new Exception($"Not enough stock for product: {product.Name}");

                product.StockQuantity -= cartItem.Quantity;
                _unitOfWork._product.Update(product);

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    SubtotalAmount = cartItem.SubtotalAmount
                });
            }

            await ClearCartAsync(customerId);
        }

        private async Task UpdateCartTotalsAsync(Cart cart)
        {
            cart.ItemCount = cart.CartItems.Sum(ci => ci.Quantity);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.SubtotalAmount);

            _unitOfWork._cart.Update(cart);
            await _unitOfWork.CompleteAsync();
        }
    }
}
