using BookFlix.DataAccess.Data;
using BookFlix.DataAccess.Repository.IRepository;
using BookFlix.Models;
using BookFlix.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookFlixWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id),
                OrderHeader = new()
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Product.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                cartObj.Count = cartObj.Count + 1;
                _unitOfWork.ShoppingCart.Update(cartObj);
                _unitOfWork.Save();
            }
            return RedirectToAction("index");
        }

        public IActionResult Minus(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                if(cartObj.Count <= 1)
                {
                    _unitOfWork.ShoppingCart.Remove(cartObj);
                    _unitOfWork.Save();
                }
                else
                {
                    cartObj.Count = cartObj.Count - 1;
                    _unitOfWork.ShoppingCart.Update(cartObj);
                    _unitOfWork.Save();
                }               
            }
            return RedirectToAction("index");
        }

        public IActionResult remove(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                _unitOfWork.ShoppingCart.Remove(cartObj);
                _unitOfWork.Save();
            }
            return RedirectToAction("index");
        }

        public async Task<IActionResult> Buy()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.Name = user.FirstName;
            ShoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = user.Address;
            

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Product.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(ShoppingCartVM shoppingCartVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            shoppingCartVM.OrderHeader.Name = user.FirstName;
            shoppingCartVM.OrderHeader.CustomerId = user.Id;
            shoppingCartVM.OrderHeader.ShippingDate = DateTime.Now.AddDays(7);
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.OrderStatus = "Processing";
            shoppingCartVM.OrderHeader.PaymentStatus = "Pending";
            var myCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id);
            double TotalPrice = 0;
            foreach (var cart in myCart)
            {
                TotalPrice += cart.Product.Price * cart.Count;
            }
            shoppingCartVM.OrderHeader.OrderTotal = TotalPrice;


            _unitOfWork.OrderHeaders.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in myCart)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Product.Price 
                };
                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.Save();

                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                Sale sale = new Sale
                {
                    BookId = cart.ProductId,
                    Date = DateTime.Now,
                    Price = cart.Product.Price,
                    Quantity = cart.Count,
                    CustomerName = user.FirstName
                };

                Ledger ledger = new Ledger()
                {
                    BookId = cart.ProductId,
                    Date = DateTime.Now,
                    Quantity = sale.Quantity,
                    Credit = sale.Price * sale.Quantity,
                };

                Profit profit = new Profit()
                {
                    BookId = cart.ProductId,
                    Quantity = sale.Quantity,
                    Price = cart.Product.Kenadam,
                    SalePrice = sale.Price,
                    BookName = cart.Product.Title,
                    Lav = (sale.Price - cart.Product.Kenadam) * sale.Quantity,
                };

                cart.Product.Quantity = cart.Product.Quantity - sale.Quantity;
                cart.Product.TotalPrice = cart.Product.TotalPrice - sale.Price*sale.Quantity;
                _unitOfWork.Product.Update(cart.Product);
                _unitOfWork.Save();
                _context.Profits.Add(profit);
                _context.Ledgers.Add(ledger);
                _context.Sales.Add(sale);
                _context.SaveChanges();

            }


            TempData["Success"] = "Order Placed Successfully";
            return RedirectToAction("Index", "MyOrder", new { area = "Customer" });
        }
    }
}
