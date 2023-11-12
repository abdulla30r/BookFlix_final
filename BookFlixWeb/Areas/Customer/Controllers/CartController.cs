using BookFlix.DataAccess.Data;
using BookFlix.DataAccess.Repository.IRepository;
using BookFlix.Models;
using BookFlix.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;

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


            var mailBody = $@"
    <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Order Confirmation</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}

        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}

        h1, p {{
            color: #333333;
        }}

        .order-details {{
            margin-top: 20px;
            border-top: 1px solid #dddddd;
            padding-top: 20px;
        }}

        .footer {{
            margin-top: 20px;
            text-align: center;
            color: #999999;
        }}
    </style>
</head>
<body>

    <div class=""container"">
        <h1>Order Confirmation</h1>
        <p>Dear {user.FirstName},</p>
        <p>Thank you for your order. We are pleased to confirm that your order has been received and is being processed.</p>

        <div class=""order-details"">
            <h2>Order Details</h2>
            <p><strong>Order Id:</strong> {shoppingCartVM.OrderHeader.Id}</p>
            <p><strong>Order Date:</strong> {shoppingCartVM.OrderHeader.OrderDate}</p>
            <p>Your ordered Items are: </p>
            {string.Join("", myCart.Select(item => $"<li>{item.Product.Title} - {item.Count} - {item.Product.Price}Tk.</li>"))}

            <p><strong>Your Total Price:</strong> {TotalPrice} </p>
            
        </div>

        <p><strong>Address:</strong>{shoppingCartVM.OrderHeader.StreetAddress},{shoppingCartVM.OrderHeader.City}-{shoppingCartVM.OrderHeader.PostalCode}</p>

        <p>For any questions or concerns, please contact our customer support.</p>

        <div class=""footer"">
            <p>Thank you for choosing BookFlix!</p>
        </div>
    </div>

</body>
</html>

";

            await SendEmailAsync(
                user.Email,
                "Order Successfull",
                mailBody);


            TempData["Success"] = "Order Placed Successfully";
            return RedirectToAction("Index", "MyOrder", new { area = "Customer" });
        }

        private async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            try
            {
                var mail = new MimeMessage();
                mail.From.Add(MailboxAddress.Parse("bookflix247@gmail.com"));
                mail.To.Add(MailboxAddress.Parse(email));
                mail.Subject = subject;
                mail.Body = new TextPart(TextFormat.Html) { Text = confirmLink };

                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("bookflix247@gmail.com", "jnmk rnop elet trcp");
                smtp.Send(mail);
                smtp.Disconnect(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
