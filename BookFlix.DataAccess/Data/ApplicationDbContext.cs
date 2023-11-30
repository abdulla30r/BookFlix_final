using BookFlix.Models;
using BookFlix.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options){ }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set;}

        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }


        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<Sale> Sales { get; set; }

        public DbSet<Profit> Profits { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

    }
}
