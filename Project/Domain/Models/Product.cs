using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Domain.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(2,2)")]
        public decimal? Discount { get; set; }

        public string ImageUrl { get; set; }

        public int StockQuantity { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }

        public virtual Category Category { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; }

        public virtual List<CartItem> CartItems { get; set; }

        public virtual List<Review> Reviews { get; set; }

        public virtual List<CustomerFavoriteProduct> FavoritedByCustomers { get; set; } = new List<CustomerFavoriteProduct>();
    }
}