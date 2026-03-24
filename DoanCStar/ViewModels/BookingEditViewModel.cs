// File: ViewModels/BookingEditViewModel.cs

using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectListItem
using DoanCStar.Models;

namespace DoanCStar.ViewModels
{
    public class BookingSeatInfo
    {
        public int SeatId { get; set; }
        public string Name { get; set; } // Tên ghế: A1, B2
        public decimal Price { get; set; }
    }

    public class BookingEditViewModel
    {
        // Thông tin Booking hiện tại
        public int BookingId { get; set; }
        public int CurrentShowTimeId { get; set; }
        public decimal CurrentTotalPrice { get; set; }

        // Thông tin hiển thị chi tiết
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public string CurrentShowTimeText { get; set; }
        public List<BookingSeatInfo> CurrentSeats { get; set; } = new List<BookingSeatInfo>();
        public string SeatNamesString { get; set; }

        // Thông tin cho Form Thay đổi (Inputs)
        public int NewShowTimeId { get; set; } // ID suất chiếu mới
        public string SelectedNewSeatIds { get; set; } // Chuỗi ID ghế mới (ví dụ: "1,2,3")
        public decimal NewTotalPrice { get; set; }

        // Danh sách tùy chọn
        public List<SelectListItem> AvailableShowTimes { get; set; }
        public List<Seat> AvailableSeatsMap { get; set; } // Map toàn bộ ghế trong phòng chiếu mới
        public List<int> BookedSeatIds { get; set; } // Ghế đã bị đặt (trừ ghế của booking này)

        public string ErrorMessage { get; set; }
    }
}