using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.Services;
using DoanCStar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace DoanCStar.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGatewayService _paymentGateway;

        public BookingController(ApplicationDbContext context,
                             IPaymentGatewayService paymentGateway)
        {
            _context = context;
            _paymentGateway = paymentGateway;
        }

        public async Task<IActionResult> SelectSeats(int showTimeId)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == showTimeId);

            if (showTime == null) return NotFound();

            var seats = await _context.Seats
                .Where(s => s.RoomId == showTime.RoomId)
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .ToListAsync();

            var bookedSeatIds = await _context.BookingSeats
                .Where(bs => bs.Booking.ShowTimeId == showTimeId)
                .Select(bs => bs.SeatId)
                .Distinct()
                .ToListAsync();

            var vm = new BookingViewModel
            {
                ShowTimeId = showTimeId,
                MovieTitle = showTime.Movie.Title,
                CinemaName = showTime.Room.Cinema.Name,
                RoomName = showTime.Room.Name,
                StartTime = showTime.StartTime.ToString("dd/MM/yyyy HH:mm"),
                EndTime = showTime.EndTime.ToString("dd/MM/yyyy HH:mm"),
                MovieId = showTime.MovieId,
                ImageUrl = showTime.Movie.ImageUrl,
                Seats = seats,
                BookedSeatIds = bookedSeatIds
            };

            return View("SelectSeats", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectConcessions(int ShowTimeId, string SelectedSeatIds, decimal TotalPrice)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == ShowTimeId);

            if (showTime == null) return NotFound();

            // Lấy tên ghế
            var seatIds = SelectedSeatIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            var seats = await _context.Seats.Where(s => seatIds.Contains(s.SeatId)).ToListAsync();

            // Sắp xếp tên ghế cho đẹp (A1, A2...)
            var seatNames = string.Join(", ", seats.OrderBy(s => s.Row).ThenBy(s => s.Number).Select(s => $"{s.Row}{s.Number}"));

            var viewModel = new BookingConcessionViewModel
            {
                ShowTimeId = ShowTimeId,
                MovieTitle = showTime.Movie.Title,
                CinemaName = showTime.Room.Cinema.Name,
                RoomName = showTime.Room.Name,
                StartTime = showTime.StartTime.ToString("dd/MM/yyyy HH:mm"),
                ImageUrl = showTime.Movie.ImageUrl,
                SelectedSeatIds = SelectedSeatIds,
                SelectedSeatNames = seatNames,
                SeatTotalPrice = TotalPrice
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Payment(int ShowTimeId, string SelectedSeatIds, string SelectedConcessions)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == ShowTimeId);

            if (showTime == null) return NotFound();

            // Tính tiền vé từ SeatId
            var seatIdList = (SelectedSeatIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var seats = await _context.Seats
                .Where(s => seatIdList.Contains(s.SeatId))
                .ToListAsync();

            var seatTotal = seats.Sum(s => s.Price);
            var seatNames = string.Join(", ", seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .Select(s => $"{s.Row}{s.Number}"));

            // Parse SelectedConcessions: "1:2,3:1"
            var concessionItems = new List<PaymentConcessionItem>();
            if (!string.IsNullOrWhiteSpace(SelectedConcessions))
            {
                var pairs = SelectedConcessions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var idQtyDict = new Dictionary<int, int>();

                foreach (var p in pairs)
                {
                    var parts = p.Split(':');
                    if (parts.Length != 2) continue;
                    if (!int.TryParse(parts[0], out var cid)) continue;
                    if (!int.TryParse(parts[1], out var qty) || qty <= 0) continue;

                    idQtyDict[cid] = qty;
                }

                var concessions = await _context.Concessions
                    .Where(c => idQtyDict.Keys.Contains(c.ConcessionId))
                    .ToListAsync();

                foreach (var c in concessions)
                {
                    var qty = idQtyDict[c.ConcessionId];
                    concessionItems.Add(new PaymentConcessionItem
                    {
                        ConcessionId = c.ConcessionId,
                        Name = c.Name,
                        Price = c.Price,
                        Quantity = qty
                    });
                }
            }

            var vm = new PaymentViewModel
            {
                ShowTimeId = ShowTimeId,
                MovieTitle = showTime.Movie.Title,
                CinemaName = showTime.Room.Cinema.Name,
                RoomName = showTime.Room.Name,
                StartTime = showTime.StartTime.ToString("dd/MM/yyyy HH:mm"),
                ImageUrl = showTime.Movie.ImageUrl,
                SelectedSeatIds = SelectedSeatIds,
                SelectedSeatNames = seatNames,
                SeatTotalPrice = seatTotal,
                Concessions = concessionItems
            };

            return View("~/Views/Booking/Payment.cshtml", vm);    // View: Views/Booking/Payment.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            // 1. Tính tiền vé từ DB
            var seatIds = (model.SelectedSeatIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            var seats = await _context.Seats.Where(s => seatIds.Contains(s.SeatId)).ToListAsync();
            var seatTotal = seats.Sum(s => s.Price);

            // 2. Tính tiền bắp nước từ DB
            decimal concessionTotal = 0m;
            if (model.Concessions != null)
            {
                var consIds = model.Concessions.Where(x => x.Quantity > 0).Select(x => x.ConcessionId).ToList();
                var consDb = await _context.Concessions.Where(c => consIds.Contains(c.ConcessionId)).ToListAsync();

                foreach (var item in model.Concessions.Where(x => x.Quantity > 0))
                {
                    var cons = consDb.FirstOrDefault(c => c.ConcessionId == item.ConcessionId);
                    if (cons != null)
                    {
                        item.Price = cons.Price;
                        concessionTotal += cons.Price * item.Quantity;
                    }
                }
            }

            // 3. Xử lý Khuyến Mãi (Tính lại trên Server)
            decimal finalDiscountAmount = 0;
            int? finalPromotionId = null;

            if (model.PromotionId.HasValue)
            {
                var promo = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.PromotionId == model.PromotionId.Value
                                           && p.Active
                                           && DateTime.Now >= p.StartDate
                                           && DateTime.Now <= p.EndDate);
                if (promo != null)
                {
                    decimal preDiscountTotal = seatTotal + concessionTotal;
                    finalDiscountAmount = (preDiscountTotal * promo.DiscountPercent) / 100m;
                    finalPromotionId = promo.PromotionId;
                }
            }

            var finalTotal = (seatTotal + concessionTotal) - finalDiscountAmount;
            if (finalTotal < 0) finalTotal = 0;

            // 4. Kiểm tra ghế trống
            var conflictSeatIds = await _context.BookingSeats
                .Where(bs => seatIds.Contains(bs.SeatId) && bs.Booking.ShowTimeId == model.ShowTimeId)
                .Select(bs => bs.SeatId).ToListAsync();

            if (conflictSeatIds.Any())
            {
                ModelState.AddModelError("", "Ghế đã có người đặt.");
                return View("Payment", model);
            }

            // 5. Lưu dữ liệu
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    UserId = userId,
                    ShowTimeId = model.ShowTimeId,
                    BookingCode = GenerateCode("BK"),
                    BookingDate = DateTime.Now,
                    TotalPrice = finalTotal,
                    PromotionId = finalPromotionId // Lưu ID khuyến mãi
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Lưu chi tiết ghế
                var pricePerSeat = seatIds.Any() ? seatTotal / seatIds.Count : 0;
                foreach (var seatId in seatIds)
                {
                    _context.BookingSeats.Add(new BookingSeat { BookingId = booking.BookingId, SeatId = seatId, Price = pricePerSeat });
                }

                // Lưu chi tiết bắp nước
                if (model.Concessions != null)
                {
                    foreach (var c in model.Concessions.Where(x => x.Quantity > 0))
                    {
                        _context.BookingConcessions.Add(new BookingConcession { BookingId = booking.BookingId, ConcessionId = c.ConcessionId, Quantity = c.Quantity, Subtotal = c.Price * c.Quantity });
                    }
                }

                // Lưu thanh toán
                // Nếu PaymentMethod null thì gán mặc định OFFLINE để tránh lỗi
                string method = string.IsNullOrEmpty(model.PaymentMethod) ? "OFFLINE" : model.PaymentMethod;

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentCode = GenerateCode("PM"),
                    PaymentMethod = method,
                    PaymentDate = DateTime.Now,
                    Amount = finalTotal,
                    Status = method == "OFFLINE" ? PaymentStatus.Completed : PaymentStatus.Pending
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 6. Cộng điểm (Đã sửa chia 1000)
                var accountType = User.FindFirstValue("AccountType");
                decimal basePercentage = 0.05m;
                decimal bonusPercentage = (accountType == "Edu") ? 0.02m : 0.00m;

                int pointsEarned = (int)((finalTotal * (basePercentage + bonusPercentage)) / 1000m);

                var userFromDb = await _context.Users.FindAsync(userId);
                if (userFromDb != null)
                {
                    userFromDb.Points += pointsEarned;
                    _context.Users.Update(userFromDb);
                    await _context.SaveChangesAsync();
                }

                // 7. Hoàn tất
                if (method == "OFFLINE")
                {
                    await tran.CommitAsync();
                    return RedirectToAction("Complete", new { id = booking.BookingId });
                }

                var returnUrl = Url.Action("PaymentReturn", "Booking", new { code = payment.PaymentCode }, Request.Scheme);
                var notifyUrl = Url.Action("PaymentNotify", "Booking", new { code = payment.PaymentCode }, Request.Scheme);
                var gatewayUrl = await _paymentGateway.CreatePaymentUrlAsync(payment, returnUrl, notifyUrl);

                await tran.CommitAsync();
                return Redirect(gatewayUrl);
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string code, bool success)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentCode == code);

            if (payment == null) return NotFound();

            if (success)
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.Now;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                // nếu muốn, có thể hủy booking hoặc giữ lại để user thanh toán lại
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Complete", new { id = payment.BookingId });
        }
        [HttpGet]
        public async Task<IActionResult> PaymentReturn(string code, string result = "success")
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentCode == code);

            if (payment == null) return NotFound();

            if (result == "success")
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.Now;
            }
            else if (result == "cancel")
            {
                payment.Status = PaymentStatus.Cancelled;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Complete", new { id = payment.BookingId });
        }

        [HttpPost]
        public async Task<IActionResult> PaymentNotify(string code, string result)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentCode == code);

            if (payment == null) return NotFound();

            if (result == "success")
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.Now;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();

            return Ok(); 
        }

        private string GenerateCode(string prefix)
        {
            return $"{prefix}{DateTime.Now:yyMMddHHmmssfff}";
        }
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.BookingConcessions).ThenInclude(bc => bc.Concession)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }
        // Trong Controllers/BookingController.cs

        public async Task<IActionResult> History()
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            // 2. Lấy lịch sử VÉ PHIM (Booking)
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.BookingConcessions).ThenInclude(bc => bc.Concession)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // 3. Lấy lịch sử BẮP NƯỚC (SnackOrder)
            // (Nếu bạn chưa có bảng SnackOrder thì có thể để list rỗng, nhưng code bạn gửi trước đó có bảng này)
            var snackOrders = await _context.SnackOrders
                .Where(s => s.UserId == userId)
                .Include(s => s.SnackOrderDetails).ThenInclude(d => d.Concession)
                .OrderByDescending(s => s.OrderDate)
                .ToListAsync();

            // 4. ĐÓNG GÓI VÀO VIEWMODEL (Bước quan trọng để sửa lỗi)
            var viewModel = new HistoryViewModel
            {
                MovieBookings = bookings,
                SnackOrders = snackOrders
            };

            // 5. Trả về ViewModel (Không phải 'bookings')
            return View(viewModel);
        }
        [HttpGet]
        public async Task<JsonResult> GetMoviesByCinema(int cinemaId)
        {
            var movies = await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.StartTime >= DateTime.Now.AddHours(-2))
                .Select(st => new
                {
                    st.Movie.MovieId,
                    st.Movie.Title
                })
                .Distinct()
                .OrderBy(m => m.Title)
                .ToListAsync();

            var result = movies.Select(m => new SelectListItem
            {
                Value = m.MovieId.ToString(),
                Text = m.Title
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetDatesByMovieAndCinema(int cinemaId, int movieId)
        {
            var dates = await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.MovieId == movieId
                          && st.StartTime >= DateTime.Now.AddHours(-2))
                .Select(st => st.StartTime.Date)  // <-- Dùng .Date thay cho DateTruncDay
                .Distinct()
                .OrderBy(d => d)
                .Take(10)
                .Select(d => d.ToString("dd/MM/yyyy"))
                .ToListAsync();

            return Json(dates);
        }
        [HttpGet]
        [Route("Admin/BookingManagement")]
        public async Task<IActionResult> BookingManagement()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View("~/Views/Admin/BookingManagement.cshtml", bookings);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = _context.Bookings
             .Include(b => b.BookingSeats)
             .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            // Lấy tất cả ghế
            var seats = _context.Seats.ToList();

            // Lấy ID những ghế hiện có trong booking
            booking.SelectedSeatIds = booking.BookingSeats
                .Select(s => s.SeatId).ToList();

            // Suất chiếu
            ViewBag.ShowTimes = new SelectList(_context.ShowTimes, "ShowTimeId", "Name", booking.ShowTimeId);

            // Ghế
            ViewBag.Seats = seats;

            return View(booking);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            return View("~/Views/Admin/BookingManagementDelete.cshtml", booking);
        }

        // POST: Admin/Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                .Include(b => b.BookingConcessions)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            // Xóa liên quan trước để tránh lỗi FK
            _context.BookingSeats.RemoveRange(booking.BookingSeats);
            _context.BookingConcessions.RemoveRange(booking.BookingConcessions);
            if (booking.Payment != null)
                _context.Payments.Remove(booking.Payment);

            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction("BookingManagement");
        }

        // POST: Admin/Booking/Edit/5
        [HttpPost]
        public IActionResult Edit(Booking booking)
        {
            var existing = _context.Bookings
                .Include(b => b.BookingSeats)
                .FirstOrDefault(b => b.BookingId == booking.BookingId);

            if (existing == null)
                return NotFound();

            // Cập nhật suất chiếu
            existing.ShowTimeId = booking.ShowTimeId;

            // Cập nhật ghế
            existing.BookingSeats.Clear();
            if (booking.SelectedSeatIds != null)
            {
                foreach (var seatId in booking.SelectedSeatIds)
                {
                    existing.BookingSeats.Add(new BookingSeat
                    {
                        BookingId = existing.BookingId,
                        SeatId = seatId
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("BookingManagement");
        }

        [HttpGet]
        public async Task<JsonResult> GetShowTimesByFilters(int cinemaId, int movieId, string date)
        {
            if (string.IsNullOrEmpty(date))
                return Json(new List<SelectListItem>());

            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var selectedDate))
                return Json(new List<SelectListItem>());

            var showtimes = await _context.ShowTimes
                .Where(st => st.Room.CinemaId == cinemaId
                          && st.MovieId == movieId
                          && st.StartTime.Date == selectedDate.Date)
                .OrderBy(st => st.StartTime)
                .Select(st => new
                {
                    st.ShowTimeId,
                    Time = st.StartTime.ToString("HH:mm"),
                    RoomName = st.Room.Name
                })
                .ToListAsync();

            var result = showtimes.Select(s => new SelectListItem
            {
                Value = s.ShowTimeId.ToString(),
                Text = $"{s.Time} - {s.RoomName}"
            }).ToList();

            return Json(result);
        }

        // POST: /Booking/CheckPromotion
        // POST: /Booking/CheckPromotion
        // POST: /Booking/CheckPromotion
        // POST: /Booking/CheckPromotion
        [HttpPost]
        public async Task<IActionResult> CheckPromotion(string promoCode, decimal currentTotal)
        {
            if (string.IsNullOrEmpty(promoCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã." });
            }

            var cleanCode = promoCode.Trim().ToUpper();

            // ===== LẤY NGÀY HIỆN TẠI (GIỜ = 00:00:00) =====
            var today = DateTime.Now.Date;
            // ==============================================

            var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == cleanCode);

            if (promo == null)
            {
                return Json(new { success = false, message = "Mã khuyến mãi không tồn tại." });
            }

            if (!promo.Active)
            {
                return Json(new { success = false, message = "Mã khuyến mãi chưa được kích hoạt." });
            }

            // ===== KIỂM TRA NGÀY (CHỈ SO SÁNH PHẦN NGÀY) =====

            // 1. Kiểm tra ngày bắt đầu
            // Ví dụ: StartDate là 02/12. Nếu hôm nay là 01/12 thì < (Chưa bắt đầu). 
            // Nếu hôm nay là 02/12 thì = (Hợp lệ).
            if (today < promo.StartDate.Date)
            {
                return Json(new { success = false, message = "Chương trình khuyến mãi chưa bắt đầu." });
            }

            // 2. Kiểm tra ngày kết thúc
            // Ví dụ: EndDate là 04/12. Nếu hôm nay là 04/12 thì = (Hợp lệ). 
            // Nếu hôm nay là 05/12 thì > (Hết hạn).
            if (today > promo.EndDate.Date)
            {
                return Json(new { success = false, message = "Mã khuyến mãi đã hết hạn." });
            }
            // ==================================================

            // Tính toán giảm giá
            decimal discountAmount = (currentTotal * promo.DiscountPercent) / 100;
            decimal newTotal = currentTotal - discountAmount;
            if (newTotal < 0) newTotal = 0;

            return Json(new
            {
                success = true,
                discountPercent = promo.DiscountPercent,
                discountAmount = discountAmount,
                newTotal = newTotal,
                promotionId = promo.PromotionId,
                message = $"Áp dụng thành công! Giảm {promo.DiscountPercent}% (-{discountAmount:N0}đ)"
            });
        }

    }

}