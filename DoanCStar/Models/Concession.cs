namespace DoanCStar.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Concession
    {
        public int ConcessionId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(5000, double.MaxValue, ErrorMessage = "Giá phải từ 5.000 VNĐ trở lên")]
        public decimal Price { get; set; }

        public bool IsCombo { get; set; }
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; }
        // Thêm vào class Concession
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public bool Active { get; set; }
    }
}
