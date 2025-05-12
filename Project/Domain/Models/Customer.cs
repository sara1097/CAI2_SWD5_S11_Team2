using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }


        public virtual User User { get; set; }
        public virtual List<Order> Orders { get; set; }
        public virtual List<Cart> Carts { get; set; }
        public virtual List<Review> Reviews { get; set; }
        public virtual List<CustomerFavoriteProduct> FavoriteProducts { get; set; } = new List<CustomerFavoriteProduct>();
    }
}
