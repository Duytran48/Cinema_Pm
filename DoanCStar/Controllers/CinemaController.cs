using DoanCStar.Data;
using DoanCStar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.Controllers
{
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CinemaController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Schedule(int cinemaId)
        {
            var cinema = await _context.Cinemas.FirstOrDefaultAsync(c => c.CinemaId == cinemaId);
            if (cinema == null) return NotFound();

            ViewBag.CinemaName = cinema.Name;
            ViewBag.CinemaId = cinemaId;

            var dangChieu = await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.Movie.Status == "Đang chiếu"
                          && st.StartTime >= DateTime.Today)
                .OrderByDescending(st => st.StartTime)
                .Select(st => st.Movie)
                .Distinct()
                .ToListAsync();

            var sapChieu = await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.Movie.Status == "Sắp chiếu"
                          && st.StartTime >= DateTime.Today)
                .OrderBy(st => st.StartTime)
                .Select(st => st.Movie)
                .Distinct()
                .ToListAsync();

            ViewBag.DangChieu = dangChieu;
            ViewBag.SapChieu = sapChieu;

            return View();
        }

        private async Task<List<Movie>> GetMoviesByCinemaAndStatus(int cinemaId, string status)
        {
            return await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.Movie.Status == status
                          && st.StartTime >= DateTime.Today)
                .Select(st => st.Movie)
                .Distinct()
                .OrderBy(m => m.Title)
                .ToListAsync();
        }
    }
}