using BookFlix.DataAccess.Repository.IRepository;
using BookFlix.Models.ViewModels;
using BookFlix.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BookFlix.DataAccess.Data;

namespace BookFlixWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin,Seller")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").OrderByDescending(p => p.Id);
            return View(objProductList);
        }

        public IActionResult Add()
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            return View(productVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(ProductVM obj, IFormFile file)
        {

            Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Title == obj.Product.Title);
            if (product != null && product.Author == obj.Product.Author && product.Publisher == obj.Product.Publisher)
            {
                product.Quantity = obj.Product.Quantity + product.Quantity;
                product.TotalPrice = obj.Product.TotalPrice + product.TotalPrice;
                product.buyPrice = product.TotalPrice / product.Quantity;
                product.Price =(int)(product.buyPrice + 0.2 * product.buyPrice);
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Stock Added Successfully";

                Balance balance1 = _context.Balances.FirstOrDefault(u => u.BookId == product.Id);
                balance1.Quantity = product.Quantity;
                balance1.UnitPrice = product.buyPrice;
                _context.Balances.Update(balance1);

                Purchase ppp = new Purchase()
                {
                    TotalPrice = obj.Product.TotalPrice,
                    BookId = product.Id,
                    BookName = obj.Product.Title,
                    Date = DateTime.Now,
                    Quantity = obj.Product.Quantity,
                };

                Ledger ledger2 = new Ledger()
                {
                    BookId = product.Id,
                    Date = DateTime.Now,
                    Debit = obj.Product.TotalPrice,
                    Quantity = obj.Product.Quantity,
                };
                _context.Ledgers.Add(ledger2);
                _context.Purchases.Add(ppp);
                _context.SaveChanges();

                return RedirectToAction("index");
            }
            else
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }
            }

            obj.Product.buyPrice = obj.Product.TotalPrice / obj.Product.Quantity;
            obj.Product.Price = (int)(obj.Product.buyPrice + obj.Product.buyPrice * 0.2);
            _unitOfWork.Product.Add(obj.Product);
            TempData["success"] = "Product Created Successfully";
            _unitOfWork.Save();

            Product myProduct = _unitOfWork.Product.GetFirstOrDefault(u => u.ImageUrl == obj.Product.ImageUrl);

            Purchase purchase = new Purchase()
            {
                TotalPrice = obj.Product.TotalPrice,
                BookId = myProduct.Id,
                BookName = obj.Product.Title,
                Date = DateTime.Now,
                Quantity = obj.Product.Quantity,
            };

            Ledger ledger = new Ledger()
            {
                BookId = myProduct.Id,
                Date = DateTime.Now,
                Debit = obj.Product.TotalPrice,
                Quantity = obj.Product.Quantity,
            };
            _context.Ledgers.Add(ledger);
            _context.Purchases.Add(purchase);

            Balance balance = new Balance()
            {
                BookId = myProduct.Id,
                BookName = obj.Product.Title,
                Quantity = myProduct.Quantity,
                UnitPrice = myProduct.buyPrice
            };

            _context.Balances.Add(balance);

            _context.SaveChanges();


            return RedirectToAction("Index");
        }


        //get
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            if (id == 0 || id == null)
            {
                return View(productVM);
            }

            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\products");
                var extension = Path.GetExtension(file.FileName);

                if (obj.Product.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

            }
            if (obj.Product.Id != 0)
            {
                _unitOfWork.Product.Update(obj.Product);
                TempData["success"] = "Product Updated Successfully";

            }

            else
            {
                _unitOfWork.Product.Add(obj.Product);
                TempData["success"] = "Product Created Successfully";

            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Product product)
        {
            if (product.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product Deleted Successfully";
            return RedirectToAction("Index");
        }

    }
}
