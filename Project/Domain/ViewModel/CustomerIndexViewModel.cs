using System.Collections.Generic;
using Domain.Models;

namespace Domain.ViewModel
{
    public class CustomerIndexViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}