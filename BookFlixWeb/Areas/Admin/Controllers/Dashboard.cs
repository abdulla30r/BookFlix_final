using BookFlix.DataAccess.Data;
using BookFlix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookFlixWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class Dashboard : Controller
    {
        private readonly ApplicationDbContext _context;

        public Dashboard(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Profit> profits = _context.Profits;
            return View(profits);
        }
        public IActionResult Ledger()
        {
            IEnumerable<Ledger> ledgers = _context.Ledgers.ToList();
            return View(ledgers);
        }

        public IActionResult Purchase()
        {
            IEnumerable<Purchase> purchases = _context.Purchases.ToList();
            return View(purchases);
        }

        public IActionResult Sale()
        {
            IEnumerable<Sale> sales = _context.Sales;
            return View(sales);
        }

        public IActionResult Balance()
        {
            IEnumerable<Balance> balances = _context.Balances.ToList();
            return View(balances);
        }

    }
}
