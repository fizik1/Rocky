using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Display order")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage ="Display order 0 dan katta bo'lishi kerak")]
        public int DisplayOrder { get; set; }
    }
}
