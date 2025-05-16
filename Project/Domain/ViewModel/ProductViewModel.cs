using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace Domain.ViewModel
{
    public class ProductViewModel
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal Discount { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }



        public string? ImageUrl { get; set; } // to store image path

        [ValidateNever]
        public IFormFile? File { get; set; } // for image upload

        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } // for dropdown
    }
}