using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BanNoiThat.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự")]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}