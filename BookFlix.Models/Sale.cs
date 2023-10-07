using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        public int BookId { get; set; }  
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
