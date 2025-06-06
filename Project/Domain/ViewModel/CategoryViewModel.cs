using System.ComponentModel.DataAnnotations;

namespace Domain.ViewModel
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
} 