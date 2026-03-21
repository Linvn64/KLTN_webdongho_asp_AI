using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BienTheSanPhamsController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public BienTheSanPhamsController(ShopDongHoDbContext context)
        {
            _context = context;
        }
        // GET: Admin/BienTheSanPhams
        public async Task<IActionResult> Index(string searchString, bool? isActive, int? thuongHieuId)
        {
            // 1. Nạp danh sách thương hiệu để hiển thị ở Dropdown lọc
            ViewBag.ThuongHieuId = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu", thuongHieuId);

            // 2. Bắt đầu query
            var query = _context.BienTheSanPhams
                .Include(b => b.MaSpNavigation)
                .ThenInclude(s => s.ThuongHieuNavigation) // Include thêm hãng để lọc
                .AsQueryable();

            // 3. Lọc theo hãng
            if (thuongHieuId.HasValue)
            {
                query = query.Where(b => b.MaSpNavigation.ThuongHieuId == thuongHieuId.Value);
            }

            // 4. Lọc theo từ khóa (Tên SP hoặc SKU)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(b => b.MaSpNavigation.TenSanPham.ToLower().Contains(searchString)
                                      || b.MaSku.ToLower().Contains(searchString));
            }

            // 5. Lọc theo trạng thái
            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = isActive;
            ViewBag.CurrentBrand = thuongHieuId;

            return View(await query.ToListAsync());
        }
        // GET: Admin/BienTheSanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bienTheSanPham = await _context.BienTheSanPhams
                .Include(b => b.MaSpNavigation)
                .Include(b => b.HinhAnhBienThes) 
                .FirstOrDefaultAsync(m => m.MaBienThe == id);

            if (bienTheSanPham == null) return NotFound();

            return View(bienTheSanPham);
        }

        // GET: Admin/BienTheSanPhams/Create
        public IActionResult Create()
        {
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "TenSanPham");

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe");

            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            var folders = Directory.GetDirectories(rootPath)
                                   .Select(Path.GetFileName)
                                   .ToList();

            ViewBag.FolderList = folders;
            return View();
        }

        // POST: Admin/BienTheSanPhams/Create
        // ... (Các phần Index, Details, Create GET giữ nguyên)

        // POST: Admin/BienTheSanPhams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int MaSp, string MauSac, int? DuongKinhMat, string? ChatLieuDay, decimal? GiaNhap, decimal? GiaBan, string? MaSku, string? ImageFolder, bool IsActive)
        {
            ModelState.Remove("MaSpNavigation");

            var isDuplicate = await _context.BienTheSanPhams.AnyAsync(x =>
        x.MaSp == MaSp &&
        x.MauSac == MauSac &&
        x.DuongKinhMat == DuongKinhMat);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Biến thể với màu sắc và kích thước này đã tồn tại cho sản phẩm này!");
            }
            // -------------------------------------

            if (GiaBan.HasValue && GiaNhap.HasValue && GiaBan < GiaNhap)
                ModelState.AddModelError("GiaBan", "Giá bán phải >= giá nhập!");

            if (!ModelState.IsValid)
            {
                ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "TenSanPham", MaSp);
                LoadFolderList(); // Hàm này bạn đã có để nạp lại folder
                return View();
            }

            var bienThe = new BienTheSanPham
            {
                MaSp = MaSp,
                MauSac = MauSac,
                DuongKinhMat = DuongKinhMat,
                ChatLieuDay = ChatLieuDay,
                GiaNhap = GiaNhap,
                GiaBan = GiaBan ?? (GiaNhap * 1.3m),
                MaSku = string.IsNullOrEmpty(MaSku) ? $"{MaSp}-{MauSac}-{DuongKinhMat}" : MaSku,
                IsActive = IsActive
            };

            _context.BienTheSanPhams.Add(bienThe);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(ImageFolder))
            {
                // Nhận lại ảnh đại diện từ hàm sync
                string? thumb = await SyncImagesFromFolder(bienThe.MaBienThe, ImageFolder);
                if (thumb != null)
                {
                    bienThe.ImageUrl = thumb;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BienTheSanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBienThe,MaSp,MauSac,DuongKinhMat,ChatLieuDay,GiaBan,GiaNhap,MaSku,ImageUrl,IsActive")] BienTheSanPham bienTheSanPham, string? ImageFolder)
        {
            if (id != bienTheSanPham.MaBienThe) return NotFound();

            ModelState.Remove("MaSpNavigation");

            // --- LOGIC 1: KIỂM TRA TRÙNG LẶP ---
            // Kiểm tra xem đã có biến thể nào khác (không phải bản thân nó) có cùng SP, Màu, Size chưa
            var isDuplicate = await _context.BienTheSanPhams.AnyAsync(x =>
                x.MaSp == bienTheSanPham.MaSp &&
                x.MauSac == bienTheSanPham.MauSac &&
                x.DuongKinhMat == bienTheSanPham.DuongKinhMat &&
                x.MaBienThe != id);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Thông số màu sắc và kích thước này đã trùng với một biến thể khác của sản phẩm!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // --- LOGIC 2: ĐỒNG BỘ ẢNH NẾU ĐỔI FOLDER ---
                    if (!string.IsNullOrEmpty(ImageFolder))
                    {
                        // Kiểm tra xem folder được chọn có khác folder hiện tại trong ImageUrl không
                        if (string.IsNullOrEmpty(bienTheSanPham.ImageUrl) || !bienTheSanPham.ImageUrl.Contains("/" + ImageFolder + "/"))
                        {
                            // Gọi hàm Sync và nhận lại URL ảnh đầu tiên
                            string? newThumb = await SyncImagesFromFolder(bienTheSanPham.MaBienThe, ImageFolder);
                            if (newThumb != null)
                            {
                                bienTheSanPham.ImageUrl = newThumb;
                            }
                        }
                    }

                    _context.Update(bienTheSanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BienTheSanPhamExists(bienTheSanPham.MaBienThe)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu không hợp lệ, nạp lại dữ liệu cho View
            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "TenSanPham", bienTheSanPham.MaSp);
            LoadFolderList(); // Bây giờ hàm này đã tồn tại
            return View(bienTheSanPham);
        }
        // Sửa lại hàm Sync: Trả về Task<string?> và KHÔNG Attach BienTheSanPham
        private async Task<string?> SyncImagesFromFolder(int maBienThe, string folderName)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", folderName);
            if (!Directory.Exists(rootPath)) return null;

            // 1. Xóa liên kết ảnh cũ
            var oldImgs = _context.HinhAnhBienThes.Where(h => h.MaBienThe == maBienThe);
            _context.HinhAnhBienThes.RemoveRange(oldImgs);
            // Lưu tạm để xóa sạch trước khi thêm mới
            await _context.SaveChangesAsync();

            // 2. Quét file mới
            var imageFiles = Directory.GetFiles(rootPath)
                .Where(f => new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => f).ToList();

            string? firstImageUrl = null;
            for (int i = 0; i < imageFiles.Count; i++)
            {
                var url = $"/hinhanhbienthe/{folderName}/{Path.GetFileName(imageFiles[i])}";
                if (i == 0) firstImageUrl = url;

                _context.HinhAnhBienThes.Add(new HinhAnhBienThe
                {
                    MaBienThe = maBienThe,
                    ImageUrl = url,
                    LaAnhChinh = (i == 0),
                    ThuTuHienThi = i + 1
                });
            }

            await _context.SaveChangesAsync();
            return firstImageUrl; // Trả về ảnh đầu tiên để hàm Edit/Create tự gán
        }
        // GET: Admin/BienTheSanPhams/Edit/5
        // GET: Admin/BienTheSanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bienTheSanPham = await _context.BienTheSanPhams.FindAsync(id);
            if (bienTheSanPham == null) return NotFound();

            ViewData["MaSp"] = new SelectList(_context.SanPhams, "MaSp", "TenSanPham", bienTheSanPham.MaSp);

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            var folders = Directory.GetDirectories(rootPath)
                                   .Select(Path.GetFileName)
                                   .ToList();
            ViewBag.FolderList = folders;

            return View(bienTheSanPham);
        }
        // POST: Admin/BienTheSanPhams/Edit/5
      
        // GET: Admin/BienTheSanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bienTheSanPham = await _context.BienTheSanPhams
                .Include(b => b.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaBienThe == id);

            if (bienTheSanPham == null) return NotFound();

            return View(bienTheSanPham);
        }

        // POST: Admin/BienTheSanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bienTheSanPham = await _context.BienTheSanPhams.FindAsync(id);
            if (bienTheSanPham != null)
            {
                var gallery = _context.HinhAnhBienThes.Where(h => h.MaBienThe == id);
                _context.HinhAnhBienThes.RemoveRange(gallery);

                _context.BienTheSanPhams.Remove(bienTheSanPham);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BienTheSanPhamExists(int id)
        {
            return _context.BienTheSanPhams.Any(e => e.MaBienThe == id);
        }



        [HttpGet]
        public IActionResult GetImagesInFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return Json(new List<string>());
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", folderName);
            if (!Directory.Exists(path)) return Json(new List<string>());
            var files = Directory.GetFiles(path).Select(Path.GetFileName)
                .Where(f => new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f).ToLower()));
            return Json(files);
        }

        private void LoadFolderList()
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            ViewBag.FolderList = Directory.GetDirectories(rootPath).Select(Path.GetFileName).ToList();
        }
    }
}