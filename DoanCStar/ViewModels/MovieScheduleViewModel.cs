// File: ViewModels/MovieScheduleViewModel.cs
using DoanCStar.Models;

namespace DoanCStar.ViewModels
{
    public class CinemaSchedule
    {
        public required Cinema Cinema { get; set; }
        public required List<ShowTime> Showtimes { get; set; }
    }

    public class MovieScheduleViewModel
    {
        public required Movie Movie { get; set; }
        public required List<CinemaSchedule> Cinemas { get; set; }
    }
}