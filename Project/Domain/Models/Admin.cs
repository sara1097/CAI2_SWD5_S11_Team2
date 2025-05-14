using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }


        public virtual User User { get; set; }

        public virtual List<Order> Orders { get; set; } = new List<Order>();
        public virtual List<Product> Products { get; set; } = new List<Product>();
        public virtual List<Category> Categories { get; set; } = new List<Category>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();

    }
}
