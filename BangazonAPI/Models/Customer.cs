﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//Author: Brittany Ramos-Janeway
//Function: The declaration of the customer type class

namespace BangazonAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
