using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.ViewModels; // Thêm using
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoanCStar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MovieManagement()
        {
            return View();
        }
        public IActionResult RevenueManagement()
        {
            return View();
        }
        public IActionResult PromotionManagement()
        {
            return View();
        }
        public IActionResult NewsManagement()
        {
            return View();
        }
        public IActionResult SystemManagement()
        {
            return View();
        }


        public async Task<IActionResult> BookingManagement()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                // ... (thêm các Include khác)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // Chuyển đổi sang ViewModel
            var viewModel = bookings.Select(b => new BookingViewModel
            {
                // ... (Logic mapping dữ liệu) ...
            }).ToList();

            return View(viewModel);
        }

        // ===== QUẢN LÝ TÀI KHOẢN (USER) 
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> RevenueManagement(DateTime? fromDate, DateTime? toDate)
        {
            // 1. Thiết lập khoảng thời gian mặc định (tháng hiện tại nếu không chọn)
            var start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = toDate ?? DateTime.Now;

            // 2. Lấy danh sách đơn hàng ĐÃ THANH TOÁN trong khoảng thời gian
            // Lưu ý: "Completed" là trạng thái thanh toán thành công
            var query = _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.BookingSeats) // Để đếm số vé
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Where(b => b.Payment != null
                         && b.Payment.Status == "Completed"
                         && b.BookingDate >= start
                         && b.BookingDate <= end.AddDays(1)) // Cộng 1 ngày để lấy hết cuối ngày
                .AsNoTracking(); // Tăng tốc độ truy vấn

            var paidBookings = await query.ToListAsync();

            // 3. Tính toán báo cáo

            // A. Theo Phim
            var movieStats = paidBookings
                .GroupBy(b => b.ShowTime.Movie.Title)
                .Select(g => new MovieRevenueItem
                {
                    MovieTitle = g.Key,
                    TicketCount = g.Sum(b => b.BookingSeats.Count),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            // B. Theo Rạp
            var cinemaStats = paidBookings
                .GroupBy(b => b.ShowTime.Room.Cinema.Name)
                .Select(g => new CinemaRevenueItem
                {
                    CinemaName = g.Key,
                    TicketCount = g.Sum(b => b.BookingSeats.Count),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            // 4. Đóng gói vào ViewModel
            var viewModel = new RevenueReportViewModel
            {
                FromDate = start,
                ToDate = end,
                TotalSystemRevenue = paidBookings.Sum(b => b.TotalPrice),
                RevenueByMovie = movieStats,
                RevenueByCinema = cinemaStats
            };

            return View("~/Views/Admin/RevenueManagement.cshtml", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("UserId,Email,Phone,Role")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Bookings");
            ModelState.Remove("FullName");

            if (ModelState.IsValid)
            {
                try
                {
                    var userFromDb = await _context.Users.FindAsync(id);
                    if (userFromDb == null)
                    {
                        return NotFound();
                    }
                    userFromDb.Email = user.Email;
                    userFromDb.Phone = user.Phone;
                    userFromDb.Role = user.Role;

                    _context.Update(userFromDb);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Lưu thay đổi thành công!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return BadRequest(ModelState);
        }

        // Trong AdminController.cs

        // Trong file: Controllers/AdminController.cs

        // Trong AdminController.cs

        // 1. ACTION GET: HIỂN THỊ TRANG SỬA (Đã sửa để trả về ViewModel)
        [HttpGet]
        public async Task<IActionResult> EditBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            // Lấy danh sách suất chiếu tương lai
            var availableShowTimes = await _context.ShowTimes
                .Where(st => st.StartTime > DateTime.Now && st.Room.CinemaId == booking.ShowTime.Room.CinemaId) // Cùng rạp
                .Include(st => st.Room)
                .OrderBy(st => st.StartTime)
                .Select(st => new SelectListItem
                {
                    Value = st.ShowTimeId.ToString(),
                    Text = $"{st.StartTime:dd/MM HH:mm} - {st.Room.Name} ({st.Price:N0}đ)",
                    Selected = st.ShowTimeId == booking.ShowTimeId
                })
                .ToListAsync();

            var viewModel = new BookingEditViewModel
            {
                BookingId = booking.BookingId,
                CurrentShowTimeId = booking.ShowTimeId,
                CurrentTotalPrice = booking.TotalPrice,
                MovieTitle = booking.ShowTime.Movie.Title,
                CinemaName = booking.ShowTime.Room.Cinema.Name,
                RoomName = booking.ShowTime.Room.Name,
                CurrentShowTimeText = booking.ShowTime.StartTime.ToString("dd/MM/yyyy HH:mm"),
                CurrentSeats = booking.BookingSeats.Select(bs => new BookingSeatInfo { SeatId = bs.SeatId, Name = bs.Seat.Row + bs.Seat.Number, Price = bs.Price }).ToList(),
                SeatNamesString = string.Join(", ", booking.BookingSeats.Select(bs => bs.Seat.Row + bs.Seat.Number)),

                AvailableShowTimes = availableShowTimes,
                NewShowTimeId = booking.ShowTimeId
            };

            return View("~/Views/Admin/Booking/Edit.cshtml", viewModel);
        }

        // 2. ACTION POST: LƯU THAY ĐỔI (Giữ nguyên logic đã gửi trước đó)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(BookingEditViewModel model)
        {
            // ... (Code xử lý lưu giống tin nhắn trước của tôi) ...
            // Để ngắn gọn, tôi giả định bạn đã có code này. 
            // Nếu chưa, hãy báo tôi gửi lại full.
            return RedirectToAction(nameof(BookingManagement));
        }

        // 3. ACTION API: TẢI SƠ ĐỒ GHẾ (QUAN TRỌNG - ĐỂ JS GỌI)
        [HttpGet]
        public async Task<JsonResult> GetSeatMap(int showTimeId)
        {
            var showTime = await _context.ShowTimes.FirstOrDefaultAsync(s => s.ShowTimeId == showTimeId);
            if (showTime == null) return Json(null);

            // Lấy tất cả ghế trong phòng
            var allSeats = await _context.Seats
                .Where(s => s.RoomId == showTime.RoomId)
                .OrderBy(s => s.Row).ThenBy(s => s.Number)
                .Select(s => new { s.SeatId, s.Row, s.Number, s.SeatType })
                .ToListAsync();

            // Lấy ghế đã đặt
            var bookedSeatIds = await _context.BookingSeats
                .Where(bs => bs.Booking.ShowTimeId == showTimeId)
                .Select(bs => bs.SeatId)
                .ToListAsync();

            return Json(new { seats = allSeats, bookedIds = bookedSeatIds, price = showTime.Price });
        }

        // QUẢN LÝ RẠP CHIẾU 
        [HttpGet]
        public async Task<IActionResult> CinemaManagement()
        {
            var cinemas = await _context.Cinemas.ToListAsync();
            return View("~/Views/Admin/Cinema/CinemaManagement.cshtml", cinemas);
        }

        // ===== HÀM HỖ TRỢ: DANH SÁCH THÀNH PHỐ =====
        private List<string> GetCityList()
        {
            return new List<string>
            {
                "Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Cần Thơ", "Đồng Nai",
                "Hải Phòng", "Quảng Ninh", "Bình Định", "Bình Dương", "Đắk Lắk",
                "Trà Vinh", "Yên Bái", "Vĩnh Long", "Bạc Liêu", "Kiên Giang", "Nghệ An"
            };
        }


        // GET: /Admin/CreateCinema
        [HttpGet]
        public IActionResult CreateCinema()
        {
      
            ViewBag.Cities = new SelectList(GetCityList());
            return View("~/Views/Admin/Cinema/CreateCinema.cshtml");
        }

        // POST: /Admin/CreateCinema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCinema([Bind("Name,Address,City,Phone")] Cinema cinema)
        {
            ModelState.Remove("Rooms");

            bool nameExists = await _context.Cinemas.AnyAsync(c => c.Name == cinema.Name);
            if (nameExists) ModelState.AddModelError("Name", $"Tên rạp '{cinema.Name}' đã tồn tại.");

            if (ModelState.IsValid)
            {
                _context.Add(cinema);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm rạp '{cinema.Name}' thành công!";
                return RedirectToAction(nameof(CinemaManagement));
            }
            ViewBag.Cities = new SelectList(GetCityList(), cinema.City);
            return View("~/Views/Admin/Cinema/CreateCinema.cshtml", cinema);
        }

        [HttpGet]
        public async Task<IActionResult> EditCinema(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null) return NotFound();

            // Truyền danh sách thành phố và chọn sẵn thành phố hiện tại
            ViewBag.Cities = new SelectList(GetCityList(), cinema.City);

            return View("~/Views/Admin/Cinema/EditCinema.cshtml", cinema);
        }

        // POST: /Admin/EditCinema/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCinema(int id, [Bind("CinemaId,Name,Address,City,Phone")] Cinema cinema)
        {
            if (id != cinema.CinemaId) return NotFound();
            ModelState.Remove("Rooms");

            if (ModelState.IsValid)
            {
                // ... (Logic update giữ nguyên) ...
                try
                {
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật rạp '{cinema.Name}' thành công!";
                }
                catch (DbUpdateConcurrencyException) { /*...*/ }

                return RedirectToAction(nameof(CinemaManagement));
            }

            // Nếu lỗi, nạp lại danh sách thành phố
            ViewBag.Cities = new SelectList(GetCityList(), cinema.City);
            return View("~/Views/Admin/Cinema/EditCinema.cshtml", cinema);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCinema(int id)
        {
            var cinema = await _context.Cinemas.FirstOrDefaultAsync(m => m.CinemaId == id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Cinema/DeleteCinema.cshtml", cinema);
        }

        [HttpPost, ActionName("DeleteCinema")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCinemaConfirmed(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema != null)
            {
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa rạp '{cinema.Name}' thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy rạp để xóa.";
            }
            return RedirectToAction(nameof(CinemaManagement));
        }

        [HttpGet]
        public async Task<IActionResult> RoomManagement(int cinemaId)
        {
            var cinema = await _context.Cinemas.FindAsync(cinemaId);
            if (cinema == null)
            {
                return NotFound("Không tìm thấy rạp chiếu.");
            }
            var rooms = await _context.Rooms
                                .Where(r => r.CinemaId == cinemaId)
                                .ToListAsync();
            ViewBag.CinemaName = cinema.Name;
            ViewBag.CinemaId = cinema.CinemaId;
            return View("~/Views/Admin/Cinema/RoomManagement.cshtml", rooms);
        }

        // thêm phòng chiếu
        [HttpGet]
        public async Task<IActionResult> CreateRoom(int cinemaId)
        {
            var cinema = await _context.Cinemas.FindAsync(cinemaId);
            if (cinema == null)
            {
                return NotFound("Không tìm thấy rạp.");
            }
            ViewBag.CinemaName = cinema.Name;
            ViewBag.CinemaId = cinemaId;
            return View("~/Views/Admin/Cinema/CreateRoom.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom([Bind("CinemaId,Name,Capacity")] Room room)
        {
            ModelState.Remove("Cinema");
            ModelState.Remove("Seats");
            ModelState.Remove("ShowTimes");

            bool nameExists = await _context.Rooms.AnyAsync(r => r.CinemaId == room.CinemaId && r.Name == room.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Phòng có tên '{room.Name}' đã tồn tại trong rạp này.");
            }

            // 2. Kiểm tra sức chứa (không quá 300)
            if (room.Capacity > 300)
            {
                ModelState.AddModelError("Capacity", "Sức chứa của phòng không được vượt quá 300 ghế.");
            }
            if (room.Capacity <= 0)
            {
                ModelState.AddModelError("Capacity", "Sức chứa phải lớn hơn 0.");
            }
            if (room.Capacity % 10 != 0)
            {
                ModelState.AddModelError("Capacity", "Sức chứa phải là số chẵn và chia hết cho 10 (ví dụ: 40, 50, 100) để đảm bảo xếp ghế đều.");
            }
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm phòng '{room.Name}' thành công!";
                return RedirectToAction(nameof(RoomManagement), new { cinemaId = room.CinemaId });
            }

            var cinema = await _context.Cinemas.FindAsync(room.CinemaId);
            ViewBag.CinemaName = cinema?.Name;
            ViewBag.CinemaId = room.CinemaId;
            return View("~/Views/Admin/Cinema/CreateRoom.cshtml", room);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _context.Rooms
                                     .Include(r => r.Cinema)
                                     .FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Cinema/EditRoom.cshtml", room);
        }

        // Sửa thông tin phòng chiếu 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, [Bind("RoomId,CinemaId,Name,Capacity")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            ModelState.Remove("Cinema");
            ModelState.Remove("Seats");
            ModelState.Remove("ShowTimes");

            // Ràng buộc tên và sức chứa (Code cũ của bạn)
            bool nameExists = await _context.Rooms.AnyAsync(r => r.CinemaId == room.CinemaId && r.Name == room.Name && r.RoomId != id);
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Phòng có tên '{room.Name}' đã tồn tại trong rạp này.");
            }
            if (room.Capacity > 300)
            {
                ModelState.AddModelError("Capacity", "Sức chứa của phòng không được vượt quá 300 ghế.");
            }
            if (room.Capacity <= 0)
            {
                ModelState.AddModelError("Capacity", "Sức chứa phải lớn hơn 0.");
            }
            if (room.Capacity % 10 != 0)
            {
                ModelState.AddModelError("Capacity", "Sức chứa phải là số chẵn và chia hết cho 10 để đảm bảo xếp ghế đều.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lấy dữ liệu cũ từ Database để so sánh
                    var roomFromDb = await _context.Rooms.FindAsync(id);
                    if (roomFromDb == null)
                    {
                        return NotFound();
                    }

                    // 2. Kiểm tra xem Sức chứa có thay đổi không?
                    if (roomFromDb.Capacity != room.Capacity)
                    {
                        // Lấy danh sách ghế hiện tại
                        var existingSeats = await _context.Seats.Where(s => s.RoomId == id).ToListAsync();

                        if (existingSeats.Any())
                        {
                            // A. Kiểm tra an toàn: Có ghế nào đã được đặt vé chưa?
                            bool isAnyBooked = await _context.BookingSeats
                                .AnyAsync(bs => existingSeats.Select(s => s.SeatId).Contains(bs.SeatId));

                            if (isAnyBooked)
                            {
                                // Nếu đã có vé bán ra, KHÔNG ĐƯỢC PHÉP đổi sức chứa (vì sẽ phải xóa ghế)
                                ModelState.AddModelError("Capacity", "Không thể thay đổi sức chứa vì phòng này đang có ghế đã được đặt vé/bán. Vui lòng xử lý các đơn hàng trước.");

                                // Trả về view để hiện lỗi
                                var cinemaInfo = await _context.Cinemas.FindAsync(room.CinemaId);
                                room.Cinema = cinemaInfo;
                                return View("~/Views/Admin/Cinema/EditRoom.cshtml", room);
                            }

                            // B. Nếu chưa có vé nào: Xóa toàn bộ ghế cũ
                            _context.Seats.RemoveRange(existingSeats);
                            TempData["InfoMessage"] = "Vì sức chứa thay đổi, toàn bộ sơ đồ ghế cũ đã được xóa. Vui lòng tạo lại sơ đồ.";
                        }
                    }

                    // 3. Cập nhật thông tin mới
                    roomFromDb.Name = room.Name;
                    roomFromDb.Capacity = room.Capacity;
                    // (CinemaId thường không đổi, nhưng nếu muốn đổi rạp thì thêm dòng dưới)
                    // roomFromDb.CinemaId = room.CinemaId; 

                    _context.Update(roomFromDb);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật phòng '{room.Name}' thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.RoomId == room.RoomId)) return NotFound(); else throw;
                }
                return RedirectToAction(nameof(RoomManagement), new { cinemaId = room.CinemaId });
            }

            // Nếu ModelState lỗi
            var cinema = await _context.Cinemas.FindAsync(room.CinemaId);
            room.Cinema = cinema;
            return View("~/Views/Admin/Cinema/EditRoom.cshtml", room);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms
                                .Include(r => r.Cinema)
                                .FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Cinema/DeleteRoom.cshtml", room);
        }

        // Xóa phòng chiếu 
        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            int cinemaId = room?.CinemaId ?? 0;

            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa phòng '{room.Name}' thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng để xóa.";
            }
            if (cinemaId > 0)
                return RedirectToAction(nameof(RoomManagement), new { cinemaId = cinemaId });
            else
                return RedirectToAction(nameof(CinemaManagement));
        }
        // Quản lý ghế 
        [HttpGet]
        public async Task<IActionResult> SeatManagement(int roomId)
        {
            var room = await _context.Rooms
                                .Include(r => r.Cinema)
                                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null) return NotFound("Không tìm thấy phòng chiếu.");

            var seats = await _context.Seats
                                .Where(s => s.RoomId == roomId)
                                .OrderBy(s => s.Row).ThenBy(s => s.Number)
                                .ToListAsync();

            var groupedSeats = seats
                .GroupBy(s => new { s.Row, s.SeatType })
                .Select(g => new SeatGroupViewModel
                {
                    Row = g.Key.Row,
                    SeatType = g.Key.SeatType,
                    MinNumber = g.Min(s => s.Number),
                    MaxNumber = g.Max(s => s.Number),
                    Count = g.Count()
                })
                .OrderBy(g => g.Row)
                .ToList();

            int currentCapacityUsed = seats.Sum(s => s.SeatType == "Sweetbox" ? 2 : 1);


            ViewBag.RoomName = room.Name;
            ViewBag.CinemaName = room.Cinema?.Name;
            ViewBag.RoomId = roomId;
            ViewBag.CinemaId = room.CinemaId;
            ViewBag.RoomCapacity = room.Capacity;

            ViewBag.CurrentCapacityUsed = currentCapacityUsed;

            return View("~/Views/Admin/Cinema/SeatManagement.cshtml", groupedSeats);
        }
        // GET: Hiển thị form tạo tự động
        [HttpGet]
        public async Task<IActionResult> GenerateSeats(int roomId)
        {
            var room = await _context.Rooms.Include(r => r.Cinema).FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null) return NotFound("Không tìm thấy phòng.");

            var seats = await _context.Seats.Where(s => s.RoomId == roomId).ToListAsync();
            int currentCapacityUsed = seats.Sum(s => s.SeatType == "Sweetbox" ? 2 : 1);

            var viewModel = new AutoGenerateSeatsViewModel
            {
                RoomId = roomId,
                RoomName = room.Name,
                CinemaName = room.Cinema?.Name,
                RoomCapacity = room.Capacity,
                CurrentSeatCount = currentCapacityUsed,
                RowCount = 6
            };

            return View("~/Views/Admin/Cinema/GenerateSeats.cshtml", viewModel);
        }

        // POST: Xử lý thuật toán sinh ghế
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSeats(AutoGenerateSeatsViewModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null) return NotFound("Không tìm thấy phòng.");

            if (room.Capacity % 10 != 0)
            {
                ModelState.AddModelError(string.Empty, $"Sức chứa hiện tại ({room.Capacity}) sẽ bị lẻ ghế khi xếp. ");
            }

            if (model.RowCount < 4 || model.RowCount > 10 || model.RowCount % 2 != 0)
            {
                ModelState.AddModelError("RowCount", "Số hàng ghế phải là 4, 6, 8 hoặc 10.");
            }

            if (ModelState.IsValid)
            {
                int totalRows = model.RowCount;
                int normalRowsCount = totalRows - 1;

                int seatsPerNormalRow = 0;
                int sweetboxSeatsCount = 0;
                bool solutionFound = false;

                for (int n = 20; n >= 1; n--)
                {

                    int capacityUsedByNormalRows = n * normalRowsCount;
                    int remainingCapacity = room.Capacity - capacityUsedByNormalRows;


                    if (remainingCapacity > 0 && remainingCapacity % 2 == 0)
                    {
                        int calculatedSweetboxCount = remainingCapacity / 2;

                        if (calculatedSweetboxCount <= 22)
                        {
                            seatsPerNormalRow = n;
                            sweetboxSeatsCount = calculatedSweetboxCount;
                            solutionFound = true;
                            break; // Tìm thấy cấu hình hợp lệ, thoát vòng lặp
                        }

                    }
                }

                if (!solutionFound)
                {
                    ModelState.AddModelError(string.Empty, $"Không tìm được cách xếp ghế hợp lệ cho sức chứa {room.Capacity} với {totalRows} hàng. Hãy thử đổi số hàng hoặc sửa sức chứa phòng.");
                }
                else
                {
                    try
                    {
                        var oldSeats = await _context.Seats.Where(s => s.RoomId == model.RoomId).ToListAsync();
                        if (oldSeats.Any())
                        {
                            bool isAnyBooked = await _context.BookingSeats.AnyAsync(bs => oldSeats.Select(es => es.SeatId).Contains(bs.SeatId));
                            if (isAnyBooked)
                            {
                                ModelState.AddModelError(string.Empty, "Không thể tạo lại sơ đồ vì phòng này đã có vé được bán.");
                                model.RoomName = room.Name; model.CinemaName = room.Cinema?.Name; model.RoomCapacity = room.Capacity; model.CurrentSeatCount = oldSeats.Count;
                                return View("~/Views/Admin/Cinema/GenerateSeats.cshtml", model);
                            }
                            _context.Seats.RemoveRange(oldSeats);
                        }

                        var newSeats = new List<Seat>();

                        int lastRowIndex = totalRows - 1;
                        int middleRowIndex = (int)Math.Floor((double)(totalRows - 1) / 2);
                        int vipEndIndex = totalRows - 2;

                        // Vòng lặp tạo hàng
                        for (int r = 0; r < totalRows; r++)
                        {
                            string rowName = ((char)('A' + r)).ToString(); 

                            if (r == lastRowIndex)
                            {
                                for (int k = 1; k <= sweetboxSeatsCount; k++)
                                {
                                    newSeats.Add(new Seat
                                    {
                                        RoomId = model.RoomId,
                                        Row = rowName,
                                        Number = k,
                                        SeatType = "Sweetbox",
                                        Price = model.PriceSweetbox
                                    });
                                }
                            }
                        
                            else
                            {
                                string type = "Standard";
                                decimal price = model.PriceStandard;

                                // Kiểm tra khoảng VIP
                                if (r >= middleRowIndex && r <= vipEndIndex)
                                {
                                    type = "VIP";
                                    price = model.PriceVIP; 
                                }

                                for (int k = 1; k <= seatsPerNormalRow; k++)
                                {
                                    newSeats.Add(new Seat
                                    {
                                        RoomId = model.RoomId,
                                        Row = rowName,
                                        Number = k,
                                        SeatType = type,
                                        Price = price 
                                    });
                                }
                            }
                        }

                        await _context.Seats.AddRangeAsync(newSeats);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Đã tạo sơ đồ tự động thành công!\n" +
                                                     $"• Sức chứa: {room.Capacity}\n" +
                                                     $"• {normalRowsCount} hàng {seatsPerNormalRow} ghế (Standard/VIP)\n" +
                                                     $"• 1 hàng {sweetboxSeatsCount} ghế đôi (Sweetbox)";

                        return RedirectToAction("SeatManagement", new { roomId = model.RoomId });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi hệ thống: " + ex.Message);
                    }
                }
            }

            model.RoomName = room.Name;
            var cinema = await _context.Cinemas.FindAsync(room.CinemaId);
            model.CinemaName = cinema?.Name;
            model.RoomCapacity = room.Capacity;
            var currentSeats = await _context.Seats.Where(s => s.RoomId == model.RoomId).ToListAsync();
            model.CurrentSeatCount = currentSeats.Sum(s => s.SeatType == "Sweetbox" ? 2 : 1);

            return View("~/Views/Admin/Cinema/GenerateSeats.cshtml", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSeatRow(int roomId, string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
            {
                TempData["ErrorMessage"] = "Tên hàng không hợp lệ.";
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }

            rowName = rowName.ToUpper();

            var seatsToDelete = await _context.Seats
                                            .Where(s => s.RoomId == roomId && s.Row == rowName)
                                            .ToListAsync();

            if (!seatsToDelete.Any())
            {
                TempData["InfoMessage"] = $"Không tìm thấy ghế nào thuộc hàng '{rowName}' để xóa.";
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }
            bool isAnyBooked = await _context.BookingSeats
                                    .AnyAsync(bs => seatsToDelete.Select(s => s.SeatId).Contains(bs.SeatId));

            if (isAnyBooked)
            {
                TempData["ErrorMessage"] = $"Không thể xóa hàng '{rowName}' vì có ghế đã được đặt. Vui lòng xử lý booking trước.";
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }

            try
            {
                _context.Seats.RemoveRange(seatsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa thành công {seatsToDelete.Count} ghế thuộc hàng '{rowName}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa hàng ghế.";
            }

            return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelectedSeatRows(int roomId, List<string> selectedRows)
        {
            if (selectedRows == null || !selectedRows.Any())
            {
                TempData["InfoMessage"] = "Vui lòng chọn ít nhất một hàng để xóa.";
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }

            // Lấy tất cả ghế thuộc các hàng đã chọn trong phòng này
            var seatsToDelete = await _context.Seats
                .Where(s => s.RoomId == roomId && selectedRows.Contains(s.Row))
                .ToListAsync();

            if (!seatsToDelete.Any())
            {
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }

            // Kiểm tra ràng buộc: Có ghế nào đã được đặt vé không?
            bool isAnyBooked = await _context.BookingSeats
                .AnyAsync(bs => seatsToDelete.Select(s => s.SeatId).Contains(bs.SeatId));

            if (isAnyBooked)
            {
                TempData["ErrorMessage"] = "Không thể xóa (các) hàng đã chọn vì có ghế đã được đặt. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
            }

            try
            {
                _context.Seats.RemoveRange(seatsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa thành công {selectedRows.Count} hàng ghế ({seatsToDelete.Count} ghế).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa các hàng ghế.";
            }

            return RedirectToAction(nameof(SeatManagement), new { roomId = roomId });
        }
    }
}