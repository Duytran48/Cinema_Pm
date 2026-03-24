using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DoanCStar.ViewModels
{
    public class ScheduleViewModel
    {
        public List<DateTime> Dates { get; set; } = new();
        public List<SelectListItem> Movies { get; set; } = new();
        public List<SelectListItem> Cinemas { get; set; } = new();
        public int? SelectedMovieId { get; set; }
        public int? SelectedCinemaId { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
    }
}