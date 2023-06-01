using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rocky_Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategoryDopDown { get; set; }
        public IEnumerable<SelectListItem> ApplicationTypeDropDown { get; set; }
    }
}
