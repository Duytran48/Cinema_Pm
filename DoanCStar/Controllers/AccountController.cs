using DoanCStar.Data;
using DoanCStar.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DoanCStar.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trong AccountController.cs

        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User) // Cần Include User để kiểm tra email
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.BookingConcessions).ThenInclude(bc => bc.Concession)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            // === TÍNH LẠI SỐ ĐIỂM ĐÃ TÍCH ĐƯỢC ĐỂ HIỂN THỊ ===
            // Logic này chỉ để hiển thị thông báo chúc mừng, không lưu vào DB (vì DB đã lưu ở ProcessPayment rồi)
            decimal rate = 0.05m;
            if (booking.User.Email.EndsWith(".edu.vn", StringComparison.OrdinalIgnoreCase))
            {
                rate = 0.07m;
            }

            // Chia 1000 để khớp với logic mới
            int pointsEarned = (int)((booking.TotalPrice * rate) / 1000m);

            ViewBag.PointsEarned = pointsEarned;
            // =================================================

            // Trả về View Complete cũ của bạn
            return View("~/Views/Booking/Complete.cshtml", booking);
        }
        // === HÀM ĐĂNG NHẬP ===
        [HttpPost]
        public async Task<IActionResult> Login(string Identifier, string Password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email == Identifier ||
                    u.Phone == Identifier ||
                    u.FullName == Identifier
                );

                if (user == null)
                {
                    return BadRequest(new { message = "Tài khoản không tồn tại. Vui lòng kiểm tra lại." });
                }

                if (BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                {
                    await SignInUserAsync(user);
                    if (user.Role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return Ok(new { redirectUrl = Url.Action("Index", "Admin") });
                    }
                    else
                    {
                        return Ok(new { redirectUrl = Url.Action("Index", "Home") });
                    }
                }

                return BadRequest(new { message = "Mật khẩu không chính xác." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string FullName,
            string Email,
            string Phone,
            string Password,
            string ConfirmPassword)
        {
            try
            {
                if (Password != ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email này đã được sử dụng. Vui lòng chọn email khác." });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

                var newUser = new User
                {
                    FullName = FullName,
                    Email = Email,
                    Phone = Phone,
                    PasswordHash = hashedPassword,
                    Role = "Customer",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await SignInUserAsync(newUser);

                return Ok(new { redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // === HÀM ĐĂNG XUẤT ===
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        // Trong AccountController.cs

        [Authorize] // Bắt buộc đăng nhập mới xem được
        [HttpGet]
        public async Task<IActionResult> Membership()
        {
            // 1. Lấy ID của người dùng hiện tại
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy thông tin User (bao gồm cả cột Points và lịch sử Booking nếu cần)
            var user = await _context.Users
                .Include(u => u.Bookings).ThenInclude(b => b.Payment) // Include nếu muốn hiện lịch sử trong trang Membership
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // 3. Trả về View Membership (đã tạo trước đó)
            return View("~/Views/Account/Membership.cshtml", user);
        }
        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName.Trim()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email.Trim()),
                new Claim(ClaimTypes.Role, user.Role.Trim()) 
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,   
                ClaimTypes.Role);  

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        [Authorize] 
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // ===== ACTION CẬP NHẬT PROFILE (POST) =====
        [HttpPost]
        [Authorize] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> UpdateProfile([Bind("UserId,FullName,Phone")] User updatedUser)
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdString, out var currentUserId))
            {
                return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." }); 
            }
            if (updatedUser.UserId != currentUserId)
            {
                return Unauthorized(new { message = "Bạn không có quyền sửa thông tin này." }); 
            }
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Bookings");
            ModelState.Remove("Email"); 

            if (ModelState.IsValid)
            {
                try
                {
                    var userFromDb = await _context.Users.FindAsync(currentUserId);
                    if (userFromDb == null)
                    {
                        return BadRequest(new { message = "Không tìm thấy tài khoản." });
                    }
                    userFromDb.FullName = updatedUser.FullName;
                    userFromDb.Phone = updatedUser.Phone;

                    _context.Update(userFromDb);
                    await _context.SaveChangesAsync();

                    await SignInUserAsync(userFromDb);

                    return Ok(new { message = "Cập nhật thông tin thành công!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest(new { message = "Lỗi khi cập nhật dữ liệu." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Đã xảy ra lỗi máy chủ." });
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { message = string.Join(" ", errors) });
        }

        // ===== ACTION ĐỔI MẬT KHẨU (POST) =====
        [HttpPost]
        [Authorize] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {

            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {

                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin." });
            }

            if (NewPassword != ConfirmPassword)
            {
                return BadRequest(new { message = "Mật khẩu mới và xác nhận không khớp." });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
            }

            var userFromDb = await _context.Users.FindAsync(userId);
            if (userFromDb == null)
            {
                return BadRequest(new { message = "Không tìm thấy tài khoản." });
            }

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, userFromDb.PasswordHash))
            {
                return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
            }
            try
            {
                userFromDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                _context.Update(userFromDb);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lưu mật khẩu mới." });
            }
        }
    }
}