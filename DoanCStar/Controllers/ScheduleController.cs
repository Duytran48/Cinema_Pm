using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DoanCStar.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var dates = Enumerable.Range(0, 7)
                                  .Select(i => today.AddDays(i))
                                  .ToList();

            // LẤY DANH SÁCH PHIM CÓ SUẤT TRONG 7 NGÀY TỚI
            var movieIdsWithShowtime = await _context.ShowTimes
                .Where(st => st.StartTime.Date >= today && st.StartTime.Date <= today.AddDays(6))
                .Select(st => st.MovieId)
                .Distinct()
                .ToListAsync();

            // DÙNG List<object> thay vì dynamic để tránh lỗi compile
            var movies = new List<object>();

            if (movieIdsWithShowtime.Any())
            {
                movies = await _context.Movies
                    .Where(m => movieIdsWithShowtime.Contains(m.MovieId))
                    .OrderBy(m => m.Title)
                    .Select(m => new { m.MovieId, m.Title })
                    .Cast<object>()
                    .ToListAsync();
            }

            // LẤY TẤT CẢ RẠP
            var cinemas = await _context.Cinemas
                .OrderBy(c => c.City)
                .ThenBy(c => c.Name)
                .Select(c => new { c.CinemaId, c.Name, c.City })
                .Cast<object>()
                .ToListAsync();

            ViewBag.Dates = dates;
            ViewBag.Movies = movies;
            ViewBag.Cinemas = cinemas;
            ViewBag.DefaultDate = today.ToString("yyyy-MM-dd");

            return View();
        }



        

        [HttpGet]
        public async Task<IActionResult> GetShowtimes(string date, int movieId = 0, int cinemaId = 0)
        {
            // === 1. Parse ngày an toàn (hỗ trợ mọi format) ===
            DateTime selectedDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(date))
            {
                var cleanDate = date.Split(' ')[0]; // bỏ phần time hoặc :yyyy-MM-dd thừa
                var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "M/d/yyyy", "yyyy/M/d" };
                DateTime.TryParseExact(cleanDate, formats, CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out selectedDate);
                selectedDate = selectedDate.Date;
            }

            // === 2. Query dữ liệu ===
            var query = _context.ShowTimes
                .AsNoTracking()
                .Include(st => st.Movie)
                .Include(st => st.Room!)
                    .ThenInclude(r => r!.Cinema)
                .Where(st => st.StartTime.Date == selectedDate.Date);

            if (movieId > 0)
                query = query.Where(st => st.MovieId == movieId);

            if (cinemaId > 0)
                query = query.Where(st => st.Room != null && st.Room.CinemaId == cinemaId);

            var showtimes = await query.ToListAsync();

            if (!showtimes.Any())
                return PartialView("_ShowtimesPartial", new List<MovieScheduleViewModel>());

            // === 3. GOM NHÓM CHUẬN: 1 phim → 1 block, 1 rạp → 1 dòng ===
            var result = showtimes
                .Where(st => st.Movie != null && st.Room?.Cinema != null)
                .GroupBy(st => st.MovieId)                    // Quan trọng: theo ID → không trùng cũng chỉ ra 1
                .Select(g => new MovieScheduleViewModel
                {
                    Movie = g.First().Movie!,
                    Cinemas = g
                        .GroupBy(st => st.Room!.CinemaId)     // 1 rạp chỉ ra 1 lần
                        .Select(cg => new CinemaSchedule
                        {
                            Cinema = cg.First().Room!.Cinema!,
                            Showtimes = cg.OrderBy(st => st.StartTime).ToList()
                        })
                        .OrderBy(c => c.Cinema.City)
                        .ThenBy(c => c.Cinema.Name)
                        .ToList()
                })
                .OrderBy(x => x.Movie.Title)
                .ToList();

            return PartialView("_ShowtimesPartial", result);
        }
    }
}