using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByCustomerAsync(int customerId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart != null)
            {
                _context.Entry(cart).Collection(c => c.CartItems).Load(); // collection
                foreach (var item in cart.CartItems)
                {
                    _context.Entry(item).Reference(ci => ci.Product).Load(); // object
                }
            }

            return cart;
        }

        public async Task<Cart> GetCartWithItemsAsync(int id)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart != null)
            {
                _context.Entry(cart).Collection(c => c.CartItems).Load(); // collection
                foreach (var item in cart.CartItems)
                {
                    _context.Entry(item).Reference(ci => ci.Product).Load(); // object
                }
            }

            return cart;
        }

        public async Task AddItemToCartAsync(int cartId, int productId, int quantity, string color, string size)
        {
            var cart = await GetCartWithItemsAsync(cartId);
            if (cart == null)
                throw new Exception("Cart not found");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new Exception("Product not found");

            var existingItem = cart.CartItems?
                .FirstOrDefault(ci => ci.ProductId == productId );

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.SubtotalAmount = existingItem.UnitPrice * existingItem.Quantity;
                existingItem.IsInStock = product.StockQuantity >= existingItem.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = (decimal)(product.Price - product.Discount),
                    SubtotalAmount = (decimal)((product.Price - product.Discount) * quantity),
                    IsInStock = product.StockQuantity >= quantity
                };

                cart.CartItems ??= new List<CartItem>();
                cart.CartItems.Add(cartItem);
            }

            cart.ItemCount = cart.CartItems.Sum(ci => ci.Quantity);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.SubtotalAmount);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            _context.Entry(cartItem).Reference(ci => ci.Cart).Load();
            _context.Entry(cartItem).Reference(ci => ci.Product).Load();
            _context.Entry(cartItem.Cart).Collection(c => c.CartItems).Load();

            if (quantity <= 0)
            {
                await RemoveItemFromCartAsync(cartItemId);
                return;
            }

            cartItem.Quantity = quantity;
            cartItem.SubtotalAmount = cartItem.UnitPrice * quantity;
            cartItem.IsInStock = cartItem.Product.StockQuantity >= quantity;

            var cart = cartItem.Cart;
            cart.ItemCount = cart.CartItems.Sum(ci => ci.Quantity);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.SubtotalAmount);

            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            _context.Entry(cartItem).Reference(ci => ci.Cart).Load();
            _context.Entry(cartItem.Cart).Collection(c => c.CartItems).Load();

            var cart = cartItem.Cart;
            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            cart.ItemCount = cart.CartItems.Sum(ci => ci.Quantity);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.SubtotalAmount);

            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cart = await GetCartWithItemsAsync(cartId);
            if (cart == null)
                throw new Exception("Cart not found");

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.ItemCount = 0;
            cart.TotalAmount = 0;

            await _context.SaveChangesAsync();
        }
    }
}
