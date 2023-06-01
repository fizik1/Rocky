using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System.Diagnostics;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;


        public HomeController(ILogger<HomeController> logger, IProductRepository prodRepo, ICategoryRepository catRepo)
        {
            _logger = logger;
            _prodRepo = prodRepo;
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Products = _prodRepo.GetAll(includeProperties:"Category,ApplicationType"),
                Categories = _catRepo.GetAll()
            };
            return View(homeVM);
        }

        public IActionResult Details(int id)
        {
            DetailsVM detailsVM = new DetailsVM
            {
                Product = _prodRepo.FirstOrDefault(u=>u.Id==id, includeProperties:"ApplicationType,Category"),
                ExistsInCard = false
            };
            List<ShoppinCart> shoppingCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            foreach(var item in shoppingCartList)
            {
                if(item.ProductId == id)
                {
                    detailsVM.ExistsInCard = true;
                }
            }
            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id) { 
            List<ShoppinCart> shoppingCartList = new List<ShoppinCart>();
            if(HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart)!=null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppinCart { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppinCart> shoppingCartList = new List<ShoppinCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppinCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppinCart>>(WC.SessionCart);
            }
            var itemToRemove = shoppingCartList.SingleOrDefault(u=>u.ProductId == id);
            if(itemToRemove != null) { 
                shoppingCartList.Remove(itemToRemove);
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
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