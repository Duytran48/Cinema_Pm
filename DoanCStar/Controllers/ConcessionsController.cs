using DoanCStar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using DoanCStar.Data;

public class ConcessionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ConcessionsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Concessions
    public async Task<IActionResult> Index()
    {
        var concessions = await _context.Concessions.ToListAsync();
        return View("~/Views/AdminConcessions/Index.cshtml", concessions);
    }

    // GET: Concessions/Create
    public IActionResult Create()
    {
        // Hiển thị form tạo mới
        // SỬA LẠI CHỖ NÀY: Bạn đã bỏ sót đường dẫn ở đây
        return View("~/Views/AdminConcessions/Create.cshtml");
    }

    // POST: Concessions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Đã đổi "concessions" thành "concession" (số ít) cho rõ nghĩa
    public async Task<IActionResult> Create(Concession concession)
    {
        // 1. Kiểm tra tên trùng
        if (_context.Concessions.Any(c => c.Name == concession.Name))
        {
            ModelState.AddModelError("Name", "Tên sản phẩm này đã tồn tại. Vui lòng chọn tên khác.");
        }
        // Kiểm tra hình ảnh bắt buộc
        if (concession.ImageFile == null)
        {
            ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh cho sản phẩm mới");
        }
        if (ModelState.IsValid)
        {
            // Xử lý upload file
            if (concession.ImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + concession.ImageFile.FileName;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "concessions");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await concession.ImageFile.CopyToAsync(fileStream);
                }
                concession.ImageUrl = "/images/concessions/" + uniqueFileName;
            }
            concession.Active = true;

            _context.Add(concession); // Sửa thành "concession" (số ít)
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // Nếu model không hợp lệ, quay lại form Create và báo lỗi
        return View("~/Views/AdminConcessions/Create.cshtml", concession); // Sửa thành "concession" (số ít)
    }

    // GET: Concessions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var concession = await _context.Concessions.FindAsync(id); // Đổi tên biến cho nhất quán
        if (concession == null)
        {
            return NotFound();
        }
        return View("~/Views/AdminConcessions/Edit.cshtml", concession);
    }

    // POST: Concessions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Concession concession)
    {
        if (id != concession.ConcessionId)
        {
            return NotFound();
        }

        // 1. Kiểm tra tên trùng (loại trừ chính sản phẩm đang sửa)
        bool isDuplicateName = await _context.Concessions.AnyAsync(c => c.Name == concession.Name && c.ConcessionId != id);
        if (isDuplicateName)
        {
            ModelState.AddModelError("Name", "Tên sản phẩm này đã được sử dụng bởi sản phẩm khác.");
        }

        // 2. Lấy dữ liệu cũ từ DB để cập nhật
        var concessionDb = await _context.Concessions.FindAsync(id);
        if (concessionDb == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // 3. Cập nhật các thông tin cơ bản
                concessionDb.Name = concession.Name;
                concessionDb.Price = concession.Price;
                concessionDb.Description = concession.Description;
                concessionDb.IsCombo = concession.IsCombo;
                // concessionDb.Active = concession.Active; // Bỏ comment nếu form có trường Active

                // 4. Xử lý ảnh (chỉ chạy nếu có file mới được chọn)
                if (concession.ImageFile != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(concessionDb.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, concessionDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + concession.ImageFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "concessions");
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await concession.ImageFile.CopyToAsync(fileStream);
                    }

                    // Cập nhật đường dẫn ảnh mới
                    concessionDb.ImageUrl = "/images/concessions/" + uniqueFileName;
                }

                // 5. Lưu thay đổi
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Concessions.Any(e => e.ConcessionId == concession.ConcessionId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // --- QUAN TRỌNG: Xử lý khi Validation thất bại ---
        // Nếu có lỗi (ví dụ: trùng tên), cần gán lại ImageUrl cũ để view hiển thị lại ảnh hiện tại
        // Vì đối tượng 'concession' gửi lên từ form có thể bị mất ImageUrl nếu binding lỗi.
        if (string.IsNullOrEmpty(concession.ImageUrl))
        {
            concession.ImageUrl = concessionDb.ImageUrl;
        }

        return View("~/Views/AdminConcessions/Edit.cshtml", concession);
    }
    // GET: Concessions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var concession = await _context.Concessions
            .FirstOrDefaultAsync(m => m.ConcessionId == id);
        if (concession == null)
        {
            return NotFound();
        }

        return View("~/Views/AdminConcessions/Delete.cshtml", concession);
    }

    // POST: Concessions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var concession = await _context.Concessions.FindAsync(id);
        if (concession == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(concession.ImageUrl))
        {
            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, concession.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _context.Concessions.Remove(concession);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}