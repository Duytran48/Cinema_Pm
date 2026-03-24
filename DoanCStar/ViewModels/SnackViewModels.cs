namespace DoanCStar.ViewModels
{
    public class SnackCartItem
    {
        public int ConcessionId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class SnackCheckoutViewModel
    {
        public List<SnackCartItem> Items { get; set; } = new List<SnackCartItem>();
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public string PaymentMethod { get; set; }

        // Dữ liệu JSON để gửi tiếp
        public string OrderDataJson { get; set; }
    }

    public class SnackSuccessViewModel
    {
        public string OrderCode { get; set; } // Mã đơn hàng
        public string PaymentCode { get; set; } // Mã thanh toán
        public DateTime OrderDate { get; set; }
        public List<SnackCartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}