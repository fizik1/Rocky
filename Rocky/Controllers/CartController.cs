using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utilitys;
using System.Linq;
using System.Security.Claims;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<ShoppinCart> shoppinCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count()>0)
            {
                shoppinCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppinCartList.Select(i=>i.ProductId).ToList();
            IEnumerable<Product> productList = _db.Product.Where(u => prodInCart.Contains(u.Id));
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
            IEnumerable<Product> productList = _db.Product.Where(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM
            {
                ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = productList
            };

            return View(ProductUserVM);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppinCart> shoppinCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppinCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            shoppinCartList.Remove(shoppinCartList.FirstOrDefault(u=>u.ProductId==id));
            HttpContext.Session.Set(WC.SessionCart, shoppinCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
