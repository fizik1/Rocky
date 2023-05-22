using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product.Include(u=>u.Category).Include(u=>u.ApplicationType);
            //foreach(var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.Id);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.Id);
            //}
            return View(objList);
        }

        //GET - UPSERT
        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString(),
            //});
            //ViewBag.CategoryDropDown = CategoryDropDown;
            //Product product = new Product();
            ProductVM productVM = new ProductVM
            {
                Product = new Product(),
                CategoryDopDown = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                }),
                ApplicationTypeDropDown = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };
            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(productVM);
                }
            }
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            //if (ModelState.IsValid)
            //{
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    //create
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, (fileName + extension)), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Iamge = fileName + extension;
                    _db.Add(productVM.Product);
                }
                else
                {
                    //update
                    var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u=>u.Id == productVM.Product.Id);
                    if(files.Count>0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                    var oldFile = Path.Combine(upload, objFromDb.Iamge);
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                    using (var fileStream = new FileStream(Path.Combine(upload, (fileName + extension)), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.Iamge = fileName + extension;
                    }
                    else
                    {
                    productVM.Product.Iamge = objFromDb.Iamge;
                    }
                    _db.Update(productVM.Product);

                }

                _db.SaveChanges();
                return RedirectToAction("Index");
            //}
            //return View(productVM);
        }


        //GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                Product product = _db.Product.Include(u=>u.ApplicationType).Include(u=>u.Category).FirstOrDefault(u=>u.Id==id);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(product);
                }
            }
        }

        //POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            var product = _db.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            var upload = _webHostEnvironment.WebRootPath+WC.ImagePath;
            var img = Path.Combine(upload, product.Iamge);
            if(System.IO.File.Exists(img))
            {
                System.IO.File.Delete(img);
            }
            _db.Remove(product);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
