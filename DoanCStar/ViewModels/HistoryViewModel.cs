using DoanCStar.Models;
using System.Collections.Generic;

namespace DoanCStar.ViewModels
{
    public class HistoryViewModel
    {
        // Danh sách vé phim đã đặt
        public List<Booking> MovieBookings { get; set; }

        // Danh sách đơn hàng bắp nước (đặt riêng lẻ)
        public List<SnackOrder> SnackOrders { get; set; }
    }
}