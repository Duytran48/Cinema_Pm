using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoanCStar.Models
{
    // Bảng lưu thông tin chung của đơn hàng bắp nước
    public class SnackOrder
    {
        [Key]
        public int SnackOrderId { get; set; }

        public string OrderCode { get; set; } // Mã đơn hàng (VD: SN2025...)
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } // Momo, VNPAY...
        public string Status { get; set; } // Completed, Pending...

        // Liên kết với User (Khách hàng)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Danh sách các món trong đơn này
        public ICollection<SnackOrderDetail> SnackOrderDetails { get; set; }
    }

    // Bảng lưu chi tiết từng món (Ví dụ: 2 Bắp, 1 Nước trong đơn hàng trên)
    public class SnackOrderDetail
    {
        [Key]
        public int DetailId { get; set; }

        public int SnackOrderId { get; set; }
        [ForeignKey("SnackOrderId")]
        public SnackOrder SnackOrder { get; set; }

        public int ConcessionId { get; set; }
        [ForeignKey("ConcessionId")]
        public Concession Concession { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; } // Giá bán tại thời điểm mua
    }
}