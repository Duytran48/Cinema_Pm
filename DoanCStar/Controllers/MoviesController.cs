using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Movie")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        //PHÂN TRANG



        // GET: /Movie
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Select(m => new MovieViewModel
                {
                    MovieId = m.MovieId,
                    Title = m.Title,
                    Duration = m.Duration,
                    Director = m.Director,
                    Genre = m.Genre,
                    Country = m.Country,
                    ImageUrl = m.ImageUrl,
                    Status = m.Status
                })
                .ToListAsync();

            return View("Index", movies); // Views/Movie/Index.cshtml
        }

        // GET: /Movie/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("Create", new MovieViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieViewModel model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View("Create", model);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/movies");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);

                model.ImageUrl = "/uploads/movies/" + fileName;
            }

            var movie = new Movie
            {
                Title = model.Title,
                Duration = model.Duration,
                Director = model.Director,
                Genre = model.Genre,
                Country = model.Country,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                TrailerUrl = model.TrailerUrl,
                Language = model.Language,
                Subtitle = model.Subtitle,
                Dubbed = model.Dubbed,
                Status = model.Status
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm phim thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Movie/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var vm = new MovieViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Duration = movie.Duration,
                Director = movie.Director,
                Genre = movie.Genre,
                Country = movie.Country,
                Description = movie.Description,
                ImageUrl = movie.ImageUrl,
                TrailerUrl = movie.TrailerUrl,
                Language = movie.Language,
                Subtitle = movie.Subtitle,
                Dubbed = movie.Dubbed,
                Status = movie.Status
            };

            return View("Details", vm);
        }

        // GET: /Movie/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var vm = new MovieViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Duration = movie.Duration,
                Director = movie.Director,
                Genre = movie.Genre,
                Country = movie.Country,
                Description = movie.Description,
                ImageUrl = movie.ImageUrl,
                TrailerUrl = movie.TrailerUrl,
                Language = movie.Language,
                Subtitle = movie.Subtitle,
                Dubbed = movie.Dubbed,
                Status = movie.Status
            };

            return View("Edit", vm);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieViewModel model, IFormFile? ImageFile)
        {
            if (id != model.MovieId) return BadRequest();
            if (!ModelState.IsValid) return View("Edit", model);

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/movies");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);

                model.ImageUrl = "/uploads/movies/" + fileName;
            }

            movie.Title = model.Title;
            movie.Duration = model.Duration;
            movie.Director = model.Director;
            movie.Genre = model.Genre;
            movie.Country = model.Country;
            movie.Description = model.Description;
            movie.ImageUrl = model.ImageUrl ?? movie.ImageUrl;
            movie.TrailerUrl = model.TrailerUrl;
            movie.Language = model.Language;
            movie.Subtitle = model.Subtitle;
            movie.Dubbed = model.Dubbed;
            movie.Status = model.Status;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Movie/Delete/6
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(movie.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", movie.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa phim thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}