using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoanCStar.ViewModels
{
    public class ShowTimeViewModel
    {
        // Chỉ dùng khi Edit/Delete
        public int? ShowTimeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phim.")]
        [Display(Name = "Phim")]
        public int MovieId { get; set; }

        [Display(Name = "Rạp")]
        public int? CinemaId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng chiếu.")]
        [Display(Name = "Phòng chiếu")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian bắt đầu.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        // EndTime sẽ được tính tự động từ Movie.Duration → không cần nhập
        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; }

      
        public decimal Price { get; set; }

        // Chỉ dùng để hiển thị (không lưu vào DB)
        [NotMapped]
        public string? MovieTitle { get; set; }

        [NotMapped]
        public string? RoomName { get; set; }

        [NotMapped]
        public string? ImageUrl { get; set; }
    }
}