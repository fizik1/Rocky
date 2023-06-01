using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IApplicationUserRepository userRepo, IProductRepository prodRepo, IInquiryDetailRepository inqDRepo, IInquiryHeaderRepository inqHRepo, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _userRepo = userRepo;
            _prodRepo = prodRepo;
            _inqDRepo = inqDRepo;
            _inqHRepo = inqHRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            List<ShoppinCart> shoppinCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppinCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppinCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            return View(productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {

            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<ShoppinCart> shoppinCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppinCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppinCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM
            {
                ApplicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = productList.ToList(),
            };

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

            var subject = "New Inquiry";
            string htmlBody = "";
            using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            {
                htmlBody = sr.ReadToEnd();
            }

            StringBuilder productListSB = new StringBuilder();
            foreach (var product in productUserVM.ProductList)
            {
                productListSB.Append($" - Name: {product.Name} <span style='font - sizeof:14px;'> (ID : {product.Id})</span> <br />");
            }

            string messageBody = string.Format(htmlBody, ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString());

            await _emailSender.SendEmailAsync(WC.AdminEmail, subject, messageBody);

            InquiryHeader inquiryHeader = new InquiryHeader
            {
                ApplicationUserId = claim.Value,
                FullName = productUserVM.ApplicationUser.FullName,
                PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                Email = productUserVM.ApplicationUser.Email,
                InquiryDate=DateTime.Now
            };
            _inqHRepo.Add(inquiryHeader);
            _inqHRepo.Save();

            foreach(var product in productUserVM.ProductList)
            {
                InquiryDetail inquiryDetail = new InquiryDetail
                {
                    InquiryHeaderId = inquiryHeader.Id,
                    ProductId = product.Id,
                };
                _inqDRepo.Add(inquiryDetail);
            }
            _inqDRepo.Save();

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
        {
            List<ShoppinCart> shoppinCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppinCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            shoppinCartList.Remove(shoppinCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppinCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
