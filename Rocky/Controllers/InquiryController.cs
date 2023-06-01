using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository inqDRepo;
        public InquiryVM 

        public InquiryController( IInquiryHeaderRepository inqHRepo,IInquiryDetailRepository _inqDRepo)
        {
            _inqHRepo = inqHRepo;
            inqDRepo = _inqDRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details()
        {

            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data=_inqHRepo.GetAll() });
        }
        #endregion
    }
}
