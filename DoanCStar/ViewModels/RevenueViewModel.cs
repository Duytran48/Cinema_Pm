using System;
using System.Collections.Generic;

namespace DoanCStar.ViewModels
{
    // Dữ liệu cho từng dòng báo cáo Phim
    public class MovieRevenueItem
    {
        public string MovieTitle { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // Dữ liệu cho từng dòng báo cáo Rạp
    public class CinemaRevenueItem
    {
        public string CinemaName { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ViewModel tổng cho trang báo cáo
    public class RevenueReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalSystemRevenue { get; set; } // Tổng thu toàn hệ thống

        public List<MovieRevenueItem> RevenueByMovie { get; set; }
        public List<CinemaRevenueItem> RevenueByCinema { get; set; }
    }
}