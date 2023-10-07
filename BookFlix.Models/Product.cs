using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BookFlix.Models.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookFlix.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        public int buyPrice { get; set; }

        public int Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 1")]
        public int Quantity { get; set; }

        [Required]
        public string Publisher { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 1")]
        public int TotalPrice { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [Required]
        [ValidateNever]
        public Category Category { get; set; }
 
    }
}
