using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//Jordan Rosas: Creating the models so we can continue working with out interruption

namespace BangazonAPI.Models
{
    public class Computer
    {
        public int id { get; set; }
        public DateTime purchaseDate { get; set; }
        public DateTime? DecommisionDate { get; set; }
        public string Make { get; set; }
        public string Manufacturer { get; set; }
    }
}
