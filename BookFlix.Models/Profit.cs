using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.Models
{
    public class Profit
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int Price {  get; set; }
        public int SalePrice { get; set; }
        public int Quantity { get; set;}
        public int Lav { get; set; }
    }
}
