using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DoanCStar.Models.ViewModels
{
    public class QuickBookingViewModel
    {
        [Display(Name = "Rạp chiếu")]
        public List<SelectListItem> Cinemas { get; set; } = new List<SelectListItem>();

        [Display(Name = "Phim")]
        public List<SelectListItem> Movies { get; set; } = new List<SelectListItem>();

        [Display(Name = "Ngày chiếu")]
        public List<SelectListItem> Dates { get; set; } = new List<SelectListItem>();

        [Display(Name = "Suất chiếu")]
        public List<SelectListItem> ShowTimes { get; set; } = new List<SelectListItem>();

        public int? SelectedCinemaId { get; set; }
        public int? SelectedMovieId { get; set; }
        public string? SelectedDate { get; set; }
        public int? SelectedShowTimeId { get; set; }
    }
}