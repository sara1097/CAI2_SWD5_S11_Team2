using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required]
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;


        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
    }
    public enum ReviewStatus
    {
        Pending,    // Waiting for admin approval
        Approved,   // Approved by admin and visible
        Rejected    // Rejected by admin and not visible
    }
}