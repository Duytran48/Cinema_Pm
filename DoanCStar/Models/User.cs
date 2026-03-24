using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DoanCStar.Models
{
    // Đảm bảo Email là duy nhất trong hệ thống
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        // Mật khẩu có thể null nếu đăng nhập bằng Microsoft/Google
        public string? PasswordHash { get; set; }

        [Phone]
        [StringLength(15)]
        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = "Customer"; // Mặc định là Customer

        // ==========================================
        // === CỘT ĐIỂM TÍCH LŨY (THÊM MỚI) ===
        // ==========================================
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm không được âm")]
        public int Points { get; set; } = 0; // Mặc định là 0 điểm khi mới tạo
        // ==========================================

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Booking> Bookings { get; set; }
        public ICollection<SnackOrder> SnackOrders { get; set; }
    }
}
