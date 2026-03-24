using System.ComponentModel.DataAnnotations; 

namespace DoanCStar.ViewModels
{
    public class AddSeatRowViewModel
    {
        public int RoomId { get; set; } 
        public string? RoomName { get; set; }
        public string? CinemaName { get; set; }
        public int RoomCapacity { get; set; }
        public int CurrentCapacityUsed { get; set; }


        // Dữ liệu form cần submit và validate
        [Required(ErrorMessage = "Vui lòng nhập tên hàng.")]
        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Tên hàng phải là một chữ cái viết hoa (A-Z).")]
        [Display(Name = "Tên Hàng")]
        public string RowName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ghế bắt đầu.")]
        [Range(1, 100, ErrorMessage = "Số ghế phải lớn hơn 0.")] 
        [Display(Name = "Số ghế bắt đầu")]
        public int StartNumber { get; set; } = 1; 

        [Required(ErrorMessage = "Vui lòng nhập số ghế kết thúc.")]
        [Range(1, 100, ErrorMessage = "Số ghế phải lớn hơn 0.")]
        [Display(Name = "Số ghế kết thúc")]
        public int EndNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại ghế.")]
        [Display(Name = "Loại ghế")]
        public string SeatType { get; set; } = "Standard"; 

    }
}