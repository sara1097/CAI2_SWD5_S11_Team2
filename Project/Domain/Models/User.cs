using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class User: IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public virtual Admin Admin { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
