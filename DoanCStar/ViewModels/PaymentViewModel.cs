using System.Collections.Generic;
using System.Linq;

namespace DoanCStar.ViewModels
{
    public class PaymentConcessionItem
    {
        public int ConcessionId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class PaymentViewModel
    {
        // Thông tin suất chiếu
        public int ShowTimeId { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public string StartTime { get; set; }
        public string ImageUrl { get; set; }

        // Ghế
        public string SelectedSeatIds { get; set; }      // "1,2,3"
        public string SelectedSeatNames { get; set; }    // "A1, A2, A3"
        public decimal SeatTotalPrice { get; set; }

        // Bắp nước
        public List<PaymentConcessionItem> Concessions { get; set; } = new();
        public decimal ConcessionTotalPrice => Concessions?.Sum(c => c.Subtotal) ?? 0;

      
        public int? PromotionId { get; set; }
        public decimal DiscountAmount { get; set; }

        // Tổng
        public decimal TotalPrice => SeatTotalPrice + ConcessionTotalPrice - DiscountAmount;

        // Phương thức thanh toán
        public string PaymentMethod { get; set; }
    }
}
