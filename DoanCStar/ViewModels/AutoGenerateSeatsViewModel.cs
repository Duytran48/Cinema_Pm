using System.ComponentModel.DataAnnotations;
namespace DoanCStar.ViewModels
{
    public class AutoGenerateSeatsViewModel
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? CinemaName { get; set; }
        public int RoomCapacity { get; set; }
        public int CurrentSeatCount { get; set; } // Số ghế hiện có (để tham khảo)

        // Input duy nhất từ người dùng: Số lượng hàng ghế muốn chia
        [Required(ErrorMessage = "Vui lòng chọn số lượng hàng ghế.")]
        [Display(Name = "Tổng số hàng ghế")]
        public int RowCount { get; set; }

        [Required]
        [Display(Name = "Giá ghế thường")]
        public decimal PriceStandard { get; set; } = 68000; // Mặc định

        [Required]
        [Display(Name = "Giá ghế VIP")]
        public decimal PriceVIP { get; set; } = 73500;

        [Required]
        [Display(Name = "Giá ghế Sweetbox (đôi)")]
        public decimal PriceSweetbox { get; set; } = 188000;
    }
}
