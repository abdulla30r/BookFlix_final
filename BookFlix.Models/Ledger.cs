using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookFlix.Models
{
    public class Ledger
    {
        [Key]
        public int SerialNo { get; set; }
        public int? Credit { get; set; }
        public int? Debit { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
