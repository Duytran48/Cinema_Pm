using DoanCStar.Data;
using DoanCStar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoanCStar.Controllers
{
    public class PromotionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Promotions
        public async Task<IActionResult> Index()
        {
            var promotions = await _context.Promotions.ToListAsync();
            // Chỉ định rõ đường dẫn view
            return View("~/Views/AdminPromotions/Index.cshtml", promotions);
        }

        // GET: Promotions/Create
        public IActionResult Create()
        {
            return View("~/Views/AdminPromotions/Create.cshtml");
        }

        // POST: Promotions/Create
        // POST: Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PromotionId,Code,Name,Description,DiscountPercent,StartDate,EndDate,Active")] Promotion promotion)
        {
            // Kiểm tra ngày
            if (promotion.StartDate < DateTime.Today) ModelState.AddModelError("StartDate", "Ngày bắt đầu không thể là ngày trong quá khứ.");
            if (promotion.EndDate < promotion.StartDate) ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            // ===== KIỂM TRA TRÙNG MÃ CODE =====
            // Chuyển mã về chữ hoa để so sánh
            promotion.Code = promotion.Code?.ToUpper();
            bool codeExists = await _context.Promotions.AnyAsync(p => p.Code == promotion.Code);
            if (codeExists)
            {
                ModelState.AddModelError("Code", $"Mã khuyến mãi '{promotion.Code}' đã tồn tại. Vui lòng chọn mã khác.");
            }
            // ==================================

            if (ModelState.IsValid)
            {
                _context.Add(promotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/AdminPromotions/Create.cshtml", promotion);
        }

        // GET: Promotions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View("~/Views/AdminPromotions/Edit.cshtml", promotion);
        }

        // POST: Promotions/Edit/5
        // POST: Promotions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Thêm "Code" vào Bind
        public async Task<IActionResult> Edit(int id, [Bind("PromotionId,Code,Name,Description,DiscountPercent,StartDate,EndDate,Active")] Promotion promotion)
        {
            if (id != promotion.PromotionId) return NotFound();

            // Kiểm tra ngày (Code cũ)
            if (promotion.StartDate < DateTime.Today) ModelState.AddModelError("StartDate", "Ngày bắt đầu không thể là ngày trong quá khứ.");
            if (promotion.EndDate < promotion.StartDate) ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            // ===== 2. KIỂM TRA TRÙNG MÃ CODE (TRỪ CHÍNH NÓ RA) =====
            if (!string.IsNullOrEmpty(promotion.Code))
            {
                promotion.Code = promotion.Code.ToUpper(); // Luôn lưu chữ hoa

                // Kiểm tra xem có khuyến mãi KHÁC (p.PromotionId != id) nào đã dùng mã này chưa
                bool codeExists = await _context.Promotions.AnyAsync(p => p.Code == promotion.Code && p.PromotionId != id);

                if (codeExists)
                {
                    ModelState.AddModelError("Code", $"Mã khuyến mãi '{promotion.Code}' đã được sử dụng bởi chương trình khác.");
                }
            }
            else
            {
                ModelState.AddModelError("Code", "Vui lòng nhập mã khuyến mãi.");
            }
            // ========================================================

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.PromotionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/AdminPromotions/Edit.cshtml", promotion);
        }

        // GET: Promotions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(m => m.PromotionId == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View("~/Views/AdminPromotions/Delete.cshtml", promotion);
        }

        // POST: Promotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionId == id);
        }
    }
}