namespace DoanCStar.ViewModels
{
    public class BookingConcessionViewModel
    {
        // Thông tin vé
        public int ShowTimeId { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public string StartTime { get; set; }
        public string ImageUrl { get; set; }

        // Dữ liệu ghế
        public string SelectedSeatIds { get; set; }
        public string SelectedSeatNames { get; set; }
        public decimal SeatTotalPrice { get; set; }
    }
}