using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int BookId { get; set; }
        public string BookName { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }

    }
}
