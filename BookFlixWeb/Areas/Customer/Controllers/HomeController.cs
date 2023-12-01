using BookFlix.DataAccess.Data;
using BookFlix.DataAccess.Repository;
using BookFlix.DataAccess.Repository.IRepository;
using BookFlix.Models;
using BookFlix.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace BookFlixWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;


        public HomeController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }



        [HttpGet]
        public IActionResult getProduct()
        {
            IEnumerable<Product> objProductList;

            objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();

            var indexModel = new IndexModel
            {
                ProductList = objProductList,
                CategoryList = objCategoryList,
            };

            return new JsonResult(Ok(indexModel));
        }


        [HttpGet]
        public IActionResult Index(string? category)
        {
            
            return View();
        }

 

        //get
        public IActionResult Details(int? id)
        {
            Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category");

            ShoppingCart cartObj = new()
            {
                Count = 1,
                Product = product,
                ProductId = product.Id

            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin"))
            {
                TempData["Error"] = "Admin can't buy Products";
                return RedirectToAction("Index");

            }
            shoppingCart.CustomerId = applicationUser.Id;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.CustomerId == applicationUser.Id && u.ProductId == shoppingCart.ProductId);
            if(cartFromDb !=null)
            {
                shoppingCart.Count = cartFromDb.Count + shoppingCart.Count;
            }
            Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == shoppingCart.ProductId, includeProperties: "Category");

            if (product.Quantity < shoppingCart.Count)
            {

                ShoppingCart cartObj = new()
                {
                    Count = 1,
                    Product = product,
                    ProductId = product.Id

                };
                TempData["Error"] = "Out of Stock";
                return View(cartObj);

            }

            if (cartFromDb != null)
            {
                cartFromDb.Count = cartFromDb.Count + shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);

            }
            else
            {
                ShoppingCart cart = new()
                {
                    ProductId = shoppingCart.ProductId,
                    CustomerId = shoppingCart.CustomerId,
                    Count = shoppingCart.Count
                };
                _unitOfWork.ShoppingCart.Add(cart);

            }

            _unitOfWork.Save();
            TempData["Success"] = "Added To Cart";
            return RedirectToAction("Index");
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> sendMessageAsync()
        {
            string msg = Request.Form["msg"];
           
            if(msg != null)
            {
                ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
                Feedback feedback = new Feedback();
                feedback.Message = msg;
                feedback.UserName = applicationUser.FirstName;

                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
                TempData["Success"] = "Thanks for your feedback";
            }
            return View("Index");
        }


        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}