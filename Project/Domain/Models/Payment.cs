using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public string Status { get; set; }

        public DateTime PaymentDate { get; set; }

        public string Currency { get; set; }


        public virtual List<Order> Orders { get; set; }
    }
}
