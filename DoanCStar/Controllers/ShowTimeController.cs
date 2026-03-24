using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoanCStar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShowTimeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShowTimeController(ApplicationDbContext db) => _db = db;

        // GET: /ShowTime
        public async Task<IActionResult> Index(int? cinemaId)
        {
            // 1. Chuẩn bị danh sách Rạp cho Dropdown lọc
            ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", cinemaId);
            ViewBag.SelectedCinemaId = cinemaId; // Lưu lại ID để dùng ở View

            // 2. Bắt đầu truy vấn
            var query = _db.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .AsQueryable(); // Để có thể nối thêm điều kiện Where

            // 3. Lọc theo Rạp (nếu có chọn)
            if (cinemaId.HasValue)
            {
                query = query.Where(s => s.Room.CinemaId == cinemaId);
            }

            // 4. Lấy dữ liệu
            var showTimes = await query
                .OrderByDescending(s => s.StartTime)
                .Select(s => new ShowTimeViewModel
                {
                    ShowTimeId = s.ShowTimeId,
                    MovieTitle = s.Movie.Title,
                    RoomName = s.Room.Name + " (" + s.Room.Cinema.Name + ")",
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                    
                })
                .ToListAsync();

            return View(showTimes);
        }

        // GET: /ShowTime/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var showTime = await _db.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id);

            if (showTime == null) return NotFound();

            var vm = new ShowTimeViewModel
            {
                ShowTimeId = showTime.ShowTimeId,
                MovieTitle = showTime.Movie?.Title,
                RoomName = showTime.Room?.Name,
                ImageUrl = showTime.Movie?.ImageUrl,
                StartTime = showTime.StartTime,
                EndTime = showTime.EndTime
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomsByCinema(int cinemaId)
        {
            var rooms = await _db.Rooms
                .Where(r => r.CinemaId == cinemaId)
                // Chọn ra ID và Tên (kèm sức chứa để dễ chọn)
                .Select(r => new { r.RoomId, Name = $"{r.Name} (Sức chứa: {r.Capacity})" })
                .ToListAsync();
            return Json(rooms);
        }


        // GET: /ShowTime/Create
        // GET: /ShowTime/Create
        public async Task<IActionResult> Create(int? cinemaId)
        {
            ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title");
            ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", cinemaId);

            // 2. Nếu có cinemaId, tải luôn danh sách phòng của rạp đó
            if (cinemaId.HasValue)
            {
                var rooms = await _db.Rooms
                    .Where(r => r.CinemaId == cinemaId)
                    .Select(r => new { r.RoomId, Name = $"{r.Name} (Sức chứa: {r.Capacity})" })
                    .ToListAsync();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "Name");
            }
            else
            {
                // Nếu chưa chọn rạp, để trống phòng
                ViewBag.Rooms = new SelectList(new List<Room>(), "RoomId", "Name");
            }

            // Truyền Model có chứa CinemaId để View tự chọn
            var vm = new ShowTimeViewModel
            {
                CinemaId = cinemaId,
             
                StartTime = DateTime.Now
                                    
            };

            return View(vm);
        }

        // POST: /ShowTime/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowTimeViewModel vm)
        {
            if (vm.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Thời gian bắt đầu phải từ thời điểm hiện tại trở đi.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title", vm.MovieId);
                ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", vm.CinemaId);

                // Nạp lại phòng nếu đã chọn rạp
                if (vm.CinemaId.HasValue)
                {
                    ViewBag.Rooms = new SelectList(await _db.Rooms.Where(r => r.CinemaId == vm.CinemaId).ToListAsync(), "RoomId", "Name", vm.RoomId);
                }
                else
                {
                    ViewBag.Rooms = new SelectList(new List<Room>(), "RoomId", "Name");
                }
                return View(vm);
            }

            var movie = await _db.Movies.FindAsync(vm.MovieId);
            if (movie == null)
            {
                ModelState.AddModelError("", "Phim không tồn tại.");
                // Nạp lại ViewBag như trên...
                return View(vm);
            }

            var endTime = vm.StartTime.AddMinutes(movie.Duration);

            // Kiểm tra trùng lịch
            bool conflict = await _db.ShowTimes.AnyAsync(s =>
                s.RoomId == vm.RoomId &&
                s.ShowTimeId != 0 &&
                vm.StartTime < s.EndTime &&
                endTime > s.StartTime);

            if (conflict)
            {
                ModelState.AddModelError("", "Phòng đã có suất chiếu trùng giờ!");
                // Nạp lại ViewBag...
                ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title", vm.MovieId);
                ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", vm.CinemaId);
                if (vm.CinemaId.HasValue)
                {
                    ViewBag.Rooms = new SelectList(await _db.Rooms.Where(r => r.CinemaId == vm.CinemaId).ToListAsync(), "RoomId", "Name", vm.RoomId);
                }
                else
                {
                    ViewBag.Rooms = new SelectList(new List<Room>(), "RoomId", "Name");
                }
                return View(vm);
            }

            var showTime = new ShowTime
            {
                MovieId = vm.MovieId,
                RoomId = vm.RoomId,
                StartTime = vm.StartTime,
                EndTime = endTime
            };

            _db.ShowTimes.Add(showTime);
            ; await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm suất chiếu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ShowTime/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var showTime = await _db.ShowTimes
                .Include(s => s.Room) // Include Room để lấy CinemaId
                .FirstOrDefaultAsync(s => s.ShowTimeId == id);

            if (showTime == null) return NotFound();

            var vm = new ShowTimeViewModel
            {
                ShowTimeId = showTime.ShowTimeId,
                MovieId = showTime.MovieId,
                RoomId = showTime.RoomId,
                // Lấy CinemaId từ Room
                CinemaId = showTime.Room.CinemaId,
                StartTime = showTime.StartTime,
                EndTime = showTime.EndTime
            };

            ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title", vm.MovieId);

            // 1. Danh sách tất cả Rạp
            ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", vm.CinemaId);

            // 2. Danh sách Phòng CỦA RẠP ĐANG CHỌN
            var roomsOfCinema = await _db.Rooms.Where(r => r.CinemaId == vm.CinemaId).ToListAsync();
            ViewBag.Rooms = new SelectList(roomsOfCinema, "RoomId", "Name", vm.RoomId);

            return View(vm);
        }

        // POST: /ShowTime/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShowTimeViewModel vm)
        {
            if (id != vm.ShowTimeId) return NotFound();
            if (vm.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Thời gian bắt đầu phải từ thời điểm hiện tại trở đi.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title", vm.MovieId);
                ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", vm.CinemaId);
                if (vm.CinemaId.HasValue)
                {
                    ViewBag.Rooms = new SelectList(await _db.Rooms.Where(r => r.CinemaId == vm.CinemaId).ToListAsync(), "RoomId", "Name", vm.RoomId);
                }
                else
                {
                    ViewBag.Rooms = new SelectList(new List<Room>(), "RoomId", "Name");
                }
                return View(vm);
            }

            var movie = await _db.Movies.FindAsync(vm.MovieId);
            // Kiểm tra movie null...

            var endTime = vm.StartTime.AddMinutes(movie.Duration);

            bool conflict = await _db.ShowTimes.AnyAsync(s =>
                s.RoomId == vm.RoomId &&
                s.ShowTimeId != id &&
                vm.StartTime < s.EndTime &&
                endTime > s.StartTime);

            if (conflict)
            {
                ModelState.AddModelError("", "Phòng đã có suất chiếu trùng giờ!");
                // Nạp lại ViewBag...
                ViewBag.Movies = new SelectList(await _db.Movies.ToListAsync(), "MovieId", "Title", vm.MovieId);
                ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "CinemaId", "Name", vm.CinemaId);
                if (vm.CinemaId.HasValue)
                {
                    ViewBag.Rooms = new SelectList(await _db.Rooms.Where(r => r.CinemaId == vm.CinemaId).ToListAsync(), "RoomId", "Name", vm.RoomId);
                }
                else
                {
                    ViewBag.Rooms = new SelectList(new List<Room>(), "RoomId", "Name");
                }
                return View(vm);
            }

            try
            {
                var showTime = await _db.ShowTimes.FindAsync(id);
                if (showTime == null) return NotFound();

                showTime.MovieId = vm.MovieId;
                showTime.RoomId = vm.RoomId;
                showTime.StartTime = vm.StartTime;
                showTime.EndTime = endTime;

                _db.Update(showTime);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật suất chiếu thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.ShowTimes.AnyAsync(s => s.ShowTimeId == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: /ShowTime/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var showTime = await _db.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id);

            if (showTime == null) return NotFound();

            var vm = new ShowTimeViewModel
            {
                ShowTimeId = showTime.ShowTimeId,
                MovieTitle = showTime.Movie?.Title,
                RoomName = showTime.Room?.Name,
                StartTime = showTime.StartTime,
                EndTime = showTime.EndTime
            };

            return View(vm);
        }

        // POST: /ShowTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra có booking chưa
            var hasBooking = await _db.BookingSeats
                .AnyAsync(bs => bs.Booking.ShowTimeId == id);

            if (hasBooking)
            {
                TempData["Error"] = "Không thể xóa suất chiếu đã có vé đặt!";
                return RedirectToAction(nameof(Index));
            }

            var showTime = await _db.ShowTimes.FindAsync(id);
            if (showTime != null)
            {
                _db.ShowTimes.Remove(showTime);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Xóa suất chiếu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}