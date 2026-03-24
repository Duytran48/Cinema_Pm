using DoanCStar.Models;
using System.Collections.Generic;

namespace DoanCStar.ViewModels
{
    public class BookingViewModel
    {
        public int ShowTimeId { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int MovieId { get; set; }
        public string ImageUrl { get; set; }
        public string PreSelectedSeatIds { get; set; }

        // GHẾ TRONG PHÒNG
        public List<Seat> Seats { get; set; }

        // DANH SÁCH GHẾ ĐÃ ĐƯỢC ĐẶT CHO SUẤT CHIẾU NÀY
        public List<int> BookedSeatIds { get; set; } = new List<int>();
    }
}
