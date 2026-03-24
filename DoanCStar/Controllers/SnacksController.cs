using Microsoft.AspNetCore.Mvc;
using DoanCStar.Data;
using DoanCStar.Models;
using DoanCStar.ViewModels; // Dùng viewmodel SnackSuccessViewModel
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoanCStar.Controllers
{
    public class SnacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SnacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Trang chọn món
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách bắp nước đang kích hoạt
            var items = await _context.Concessions.Where(c => c.Active).ToListAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string OrderDataJson, string PaymentMethod) // Bỏ tham số TotalAmount từ client
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            // 2. Tạo đơn hàng (Tạm thời để TotalAmount = 0, sẽ cập nhật sau)
            var order = new SnackOrder
            {
                UserId = userId,
                OrderCode = "SN" + DateTime.Now.ToString("yyMMddHHmmss"),
                OrderDate = DateTime.Now,
                TotalAmount = 0, // <--- Tạm thời để 0
                PaymentMethod = PaymentMethod,
                Status = "Completed"
            };

            _context.SnackOrders.Add(order);
            await _context.SaveChangesAsync(); // Lưu để lấy ID đơn hàng

            // 3. Xử lý chi tiết và TÍNH TỔNG TIỀN TẠI SERVER
            decimal calculatedTotal = 0; // Biến dùng để tính tổng tiền thực tế

            if (!string.IsNullOrEmpty(OrderDataJson))
            {
                var pairs = OrderDataJson.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int qty) && qty > 0)
                    {
                        var product = await _context.Concessions.FindAsync(id);
                        if (product != null)
                        {
                            // Lưu chi tiết
                            var detail = new SnackOrderDetail
                            {
                                SnackOrderId = order.SnackOrderId,
                                ConcessionId = id,
                                Quantity = qty,
                                Price = product.Price // Lấy giá gốc từ DB
                            };
                            _context.SnackOrderDetails.Add(detail);

                            // Cộng dồn vào tổng tiền
                            calculatedTotal += (product.Price * qty);
                        }
                    }
                }

                // Lưu các chi tiết món ăn
                await _context.SaveChangesAsync();
            }

            // 4. Cập nhật lại Tổng tiền chính xác vào Đơn hàng
            order.TotalAmount = calculatedTotal;
            _context.SnackOrders.Update(order);
            await _context.SaveChangesAsync();

            // 5. Chuyển hướng đến trang thành công
            return RedirectToAction("OrderSuccess", new { id = order.SnackOrderId });
        }

        // 3. Trang thông báo thành công
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.SnackOrders
                .Include(o => o.SnackOrderDetails)
                    .ThenInclude(d => d.Concession)
                .FirstOrDefaultAsync(o => o.SnackOrderId == id);

            if (order == null) return RedirectToAction("Index");

            // Đổ dữ liệu ra ViewModel để hiển thị
            var viewModel = new SnackSuccessViewModel
            {
                OrderCode = order.OrderCode,
                PaymentCode = "PM" + order.SnackOrderId, // Mã thanh toán giả lập
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Items = order.SnackOrderDetails.Select(d => new SnackCartItem
                {
                    Name = d.Concession.Name,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            };

            return View(viewModel);
        }
    }
}