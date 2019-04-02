using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
/*JORDAN ROSAS: Creating the model for the Payment Type referencing the ERD from Andy in the slack channel. 
Defining what makes up a payment type. */
namespace BangazonAPI.Models
{
    public class PaymentType
    {
        public int id { get; set; }

        [Required]
        public string Name { get; set; }
        public int AcctNumber { get; set; }
        public int CustomerId { get; set; }
    }
}
