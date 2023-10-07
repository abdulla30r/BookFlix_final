using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.Models
{
    public class Balance
    {
        [Key]
        public int id { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }

        public int Quantity { get; set; }

        public double UnitPrice { get; set; }
    }
}
