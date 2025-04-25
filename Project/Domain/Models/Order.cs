using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public string OrderNumber { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public string PaymentStatus { get; set; }

        public string ShippingAddress { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime OrderDate { get; set; }

        [ForeignKey("Payment")]
        public int? PaymentId { get; set; }


        public virtual Customer Customer { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual List<OrderItem> OrderItems { get; set; }
    }
}
