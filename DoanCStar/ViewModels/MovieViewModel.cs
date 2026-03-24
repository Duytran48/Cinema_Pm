// ViewModels/MovieViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace DoanCStar.ViewModels
{
    public class MovieViewModel
    {
        public int? MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phim")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập thời lượng")]
        [Range(1, 500)]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đạo diễn")]
        public string Director { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập thể loại")]
        public string Genre { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập quốc gia")]
        public string Country { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? TrailerUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngôn ngữ")]
        public string Language { get; set; } = null!;

        public bool Subtitle { get; set; }
        public bool Dubbed { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Status { get; set; } = null!;
    }
}