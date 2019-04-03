using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//Jordan Rosas: Creating the models so we can continue working with out interruption
namespace BangazonAPI.Models
{
    public class Department
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
    }
}
