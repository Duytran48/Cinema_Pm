using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DoanCStar.ViewModels; 
using System;
using System.Linq;
using System.Collections.Generic;

namespace DoanCStar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public IActionResult About()
        {
            return View();
        }
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

    
        [HttpGet]
        public async Task<IActionResult> MovieDetails(int id)
        {
            // 1. Lấy thông tin phim
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            // 2. Lấy các suất chiếu trong 3 ngày tới (để hiển thị bên dưới nếu cần)
            var today = DateTime.Today;
            var threeDaysLater = today.AddDays(3);

            var showTimes = await _context.ShowTimes
                .Where(st => st.MovieId == id && st.StartTime >= today && st.StartTime < threeDaysLater)
                .Include(st => st.Room)
                .ThenInclude(r => r.Cinema)
                .OrderBy(st => st.StartTime)
                .ToListAsync();

            // 3. Đóng gói vào ViewModel
            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                ShowTimes = showTimes
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Movies(string status = "Đang chiếu")
        {
            if (!new[] { "Đang chiếu", "Sắp chiếu" }.Contains(status))
                return NotFound();

            var query = _context.Movies.Where(m => m.Status == status);

            if (status == "Đang chiếu")
            {
                query = query.OrderByDescending(m => m.MovieId); // phim mới thêm lên đầu
            }
            else // Sắp chiếu
            {
                query = query.OrderBy(m => m.MovieId); // hoặc bạn có thể sắp theo Title nếu muốn
                                                       
            }

            var movies = await query.ToListAsync();

            ViewBag.SectionTitle = status == "Đang chiếu"
                ? "TẤT CẢ PHIM ĐANG CHIẾU"
                : "TẤT CẢ PHIM SẮP CHIẾU";

            return View(movies);
        }
        public IActionResult Booking()
        {
            ViewData["Title"] = "Đặt Vé Nhanh - CGStar";
            return View();
        }
    }
}
