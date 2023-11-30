using BookFlix.DataAccess.Data;
using BookFlix.DataAccess.Repository.IRepository;
using BookFlix.Models;
using BookFlix.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookFlixWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

         public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Feedback> feedbacks= _context.Feedbacks;
            return View(feedbacks);
        }
    }
}
