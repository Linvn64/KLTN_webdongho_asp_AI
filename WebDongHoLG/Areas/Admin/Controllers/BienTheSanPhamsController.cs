using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Chứa IFormFile để nhận file upload
using System.IO; // Chứa lệnh Directory để tạo Folder
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

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();
            return Json(new
            {
                tenSanPham = sp.TenSanPham,
                doiTuong = sp.DoiTuong ?? ""
            });
        }
        // GET: Admin/BienTheSanPhams
        public async Task<IActionResult> Index(string searchString, bool? isActive, int? thuongHieuId)
        {
            ViewBag.ThuongHieuId = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu", thuongHieuId);
            var query = _context.BienTheSanPhams.Include(b => b.MaSpNavigation).ThenInclude(s => s.ThuongHieuNavigation).AsQueryable();

            if (thuongHieuId.HasValue) query = query.Where(b => b.MaSpNavigation.ThuongHieuId == thuongHieuId.Value);
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(b => b.MaSpNavigation.TenSanPham.ToLower().Contains(searchString) || b.MaSku.ToLower().Contains(searchString));
            }
            if (isActive.HasValue) query = query.Where(b => b.IsActive == isActive.Value);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = isActive;
            ViewBag.CurrentBrand = thuongHieuId;

            return View(await query.ToListAsync());
        }

        // GET: Admin/BienTheSanPhams/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var bienTheSanPham = await _context.BienTheSanPhams.Include(b => b.MaSpNavigation).Include(b => b.HinhAnhBienThes).FirstOrDefaultAsync(m => m.MaBienThe == id);
            return bienTheSanPham == null ? NotFound() : View(bienTheSanPham);
        }

        // GET: Admin/BienTheSanPhams/Create
        public IActionResult Create(int? maSp)
        {
            ViewData["MaSp"] = new SelectList(
                _context.SanPhams.Select(s => new {
                    s.MaSp,
                    TenHienThi = s.TenSanPham + (string.IsNullOrEmpty(s.DoiTuong) ? "" : " [" + s.DoiTuong + "]")
                }),
                "MaSp", "TenHienThi",
                maSp 
            );
            ViewBag.SelectedMaSp = maSp; 
            LoadFolderList();
            return View();
        }
        // POST: Admin/BienTheSanPhams/Create


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int MaSp, string MauSac, int? DuongKinhMat, string? ChatLieuDay, decimal? GiaNhap, decimal? GiaBan, string? MaSku, string? ImageFolder, bool IsActive, string? MainImage, string? ImageOrders, List<IFormFile> uploadFiles)
        {
            // DEBUG
            Console.WriteLine($"ImageFolder: '{ImageFolder}'");
            Console.WriteLine($"MainImage: '{MainImage}'");
            Console.WriteLine($"uploadFiles: {uploadFiles?.Count ?? 0}");

            var normalizedFolder = ImageFolder?
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString());

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", normalizedFolder ?? "");

            Console.WriteLine($"folderPath: '{folderPath}'");
            Console.WriteLine($"folderPath exists: {Directory.Exists(folderPath)}");

            ModelState.Remove("MaSpNavigation");

            var isDuplicate = await _context.BienTheSanPhams.AnyAsync(x =>
                x.MaSp == MaSp && x.MauSac == MauSac && x.DuongKinhMat == DuongKinhMat);
            if (isDuplicate)
                ModelState.AddModelError("", "Biến thể với màu sắc và kích thước này đã tồn tại!");

            if (!ModelState.IsValid)
            {
                ViewData["MaSp"] = new SelectList(
                    _context.SanPhams.Select(s => new {
                        s.MaSp,
                        TenHienThi = s.TenSanPham + (string.IsNullOrEmpty(s.DoiTuong) ? "" : " [" + s.DoiTuong + "]")
                    }), "MaSp", "TenHienThi", MaSp);
                LoadFolderList();
                return View();
            }

            if (!string.IsNullOrEmpty(normalizedFolder))
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine($"Created folder: {folderPath}");
                }

                if (uploadFiles != null && uploadFiles.Count > 0)
                {
                    foreach (var file in uploadFiles)
                    {
                        if (file.Length > 0)
                        {
                            var filePath = Path.Combine(folderPath, file.FileName);
                            Console.WriteLine($"Saving file: {filePath}");
                            using var stream = new FileStream(filePath, FileMode.Create);
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }

            var urlFolder = ImageFolder?.Replace("\\", "/");

            var bienThe = new BienTheSanPham
            {
                MaSp = MaSp,
                MauSac = MauSac,
                DuongKinhMat = DuongKinhMat,
                ChatLieuDay = ChatLieuDay,
                GiaNhap = GiaNhap,
                GiaBan = GiaBan ?? (GiaNhap * 1.3m),
                MaSku = MaSku,
                IsActive = IsActive,
                ImageUrl = !string.IsNullOrEmpty(MainImage) && !string.IsNullOrEmpty(urlFolder)
                                ? $"/hinhanhbienthe/{urlFolder}/{MainImage}"
                                : null
            };

            Console.WriteLine($"ImageUrl sẽ lưu: '{bienThe.ImageUrl}'");

            _context.BienTheSanPhams.Add(bienThe);
            await _context.SaveChangesAsync();

            Console.WriteLine($"MaBienThe vừa tạo: {bienThe.MaBienThe}");

            if (!string.IsNullOrEmpty(ImageFolder))
            {
                await ProcessFolderImages(bienThe.MaBienThe, ImageFolder, MainImage, ImageOrders);
            }

            var khoExists = await _context.Khos.AnyAsync(k => k.MaBienThe == bienThe.MaBienThe);
            if (!khoExists)
            {
                _context.Khos.Add(new Kho
                {
                    MaBienThe = bienThe.MaBienThe,
                    SoLuongTon = 0
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var bienTheSanPham = await _context.BienTheSanPhams.FindAsync(id);
            if (bienTheSanPham == null) return NotFound();

            ViewData["MaSp"] = new SelectList(
                _context.SanPhams.Select(s => new {
                    s.MaSp,
                    TenHienThi = s.TenSanPham + (string.IsNullOrEmpty(s.DoiTuong) ? "" : " [" + s.DoiTuong + "]")
                }),
                "MaSp", "TenHienThi",
                bienTheSanPham.MaSp // giá trị đang chọn
            );
            LoadFolderList();
            return View(bienTheSanPham);
        }
        // POST: Admin/BienTheSanPhams/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBienThe,MaSp,MauSac,DuongKinhMat,ChatLieuDay,GiaBan,GiaNhap,MaSku,ImageUrl,IsActive")] BienTheSanPham bienTheSanPham, string? ImageFolder, string? MainImage, string? ImageOrders, List<IFormFile> uploadFiles)
        {
            if (id != bienTheSanPham.MaBienThe) return NotFound();
            ModelState.Remove("MaSpNavigation");

            var isDuplicate = await _context.BienTheSanPhams.AnyAsync(x =>
                x.MaSp == bienTheSanPham.MaSp &&
                x.MauSac == bienTheSanPham.MauSac &&
                x.DuongKinhMat == bienTheSanPham.DuongKinhMat &&
                x.MaBienThe != id);
            if (isDuplicate)
                ModelState.AddModelError("", "Thông số này đã trùng với một biến thể khác!");

            if (!ModelState.IsValid)
            {
                ViewData["MaSp"] = new SelectList(
                    _context.SanPhams.Select(s => new {
                        s.MaSp,
                        TenHienThi = s.TenSanPham + (string.IsNullOrEmpty(s.DoiTuong) ? "" : " [" + s.DoiTuong + "]")
                    }), "MaSp", "TenHienThi", bienTheSanPham.MaSp);
                LoadFolderList();
                return View(bienTheSanPham);
            }

            try
            {
                var normalizedFolder = ImageFolder?
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString());

                // 1. Upload file mới nếu có
                if (!string.IsNullOrEmpty(normalizedFolder) && uploadFiles != null && uploadFiles.Count > 0)
                {
                    var folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", normalizedFolder);

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    foreach (var file in uploadFiles)
                    {
                        if (file.Length > 0)
                        {
                            var filePath = Path.Combine(folderPath, file.FileName);
                            using var stream = new FileStream(filePath, FileMode.Create);
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                var urlFolder = ImageFolder?.Replace("\\", "/");

                // 2. Cập nhật ImageUrl theo ảnh chính được chọn
                if (!string.IsNullOrEmpty(MainImage) && !string.IsNullOrEmpty(urlFolder))
                    bienTheSanPham.ImageUrl = $"/hinhanhbienthe/{urlFolder}/{MainImage}";

                // 3. Lưu thông tin biến thể (CHỈ GỌI 1 LẦN)
                _context.Update(bienTheSanPham);
                await _context.SaveChangesAsync();

                // 4. Đồng bộ ảnh trong DB
                if (!string.IsNullOrEmpty(ImageFolder))
                {
                    var oldImages = _context.HinhAnhBienThes.Where(h => h.MaBienThe == id);
                    _context.HinhAnhBienThes.RemoveRange(oldImages);
                    await _context.SaveChangesAsync();

                    await ProcessFolderImages(bienTheSanPham.MaBienThe, ImageFolder, MainImage, ImageOrders);
                }

                // 5. Đảm bảo bản ghi Kho tồn tại
                var khoExists = await _context.Khos.AnyAsync(k => k.MaBienThe == bienTheSanPham.MaBienThe);
                if (!khoExists)
                {
                    _context.Khos.Add(new Kho
                    {
                        MaBienThe = bienTheSanPham.MaBienThe,
                        SoLuongTon = 0
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BienTheSanPhamExists(bienTheSanPham.MaBienThe)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }
        private async Task ProcessFolderImages(int maBienThe, string folderName, string? mainImage, string? imageOrders)
        {
            var normalizedFolder = folderName
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString());

            var rootPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", normalizedFolder);

            Console.WriteLine($"ProcessFolderImages - rootPath: '{rootPath}'");
            Console.WriteLine($"ProcessFolderImages - exists: {Directory.Exists(rootPath)}");
            Console.WriteLine($"ProcessFolderImages - mainImage: '{mainImage}'");

            if (!Directory.Exists(rootPath)) return;

            var allowedExt = new[] { ".jpg", ".png", ".jpeg", ".webp" };
            var imageFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
                .Where(f => allowedExt.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => f.Replace(rootPath, "").TrimStart('\\', '/').Replace("\\", "/"))
                .ToList();

            Console.WriteLine($"ProcessFolderImages - found {imageFiles.Count} files: {string.Join(", ", imageFiles)}");

            var orderMap = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(imageOrders))
            {
                try { orderMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(imageOrders) ?? new(); }
                catch { }
            }

            var urlFolder = folderName.Replace("\\", "/");

            foreach (var fileName in imageFiles)
            {
                bool isMain = (fileName == mainImage);
                Console.WriteLine($"  File: '{fileName}' | isMain: {isMain} (so sánh với '{mainImage}')");

                _context.HinhAnhBienThes.Add(new HinhAnhBienThe
                {
                    MaBienThe = maBienThe,
                    ImageUrl = $"/hinhanhbienthe/{urlFolder}/{fileName}",
                    LaAnhChinh = isMain,
                    ThuTuHienThi = orderMap.ContainsKey(fileName) ? orderMap[fileName] : 99
                });
            }
            await _context.SaveChangesAsync();
            Console.WriteLine($"ProcessFolderImages - saved {imageFiles.Count} records to DB");
        }
        private async Task<string?> SyncImagesFromFolder(int maBienThe, string folderName)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", folderName);
            if (!Directory.Exists(rootPath)) return null;

            var oldImgs = _context.HinhAnhBienThes.Where(h => h.MaBienThe == maBienThe);
            _context.HinhAnhBienThes.RemoveRange(oldImgs);
            await _context.SaveChangesAsync();

            var imageFiles = Directory.GetFiles(rootPath).Where(f => new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f).ToLower())).OrderBy(f => f).ToList();
            string? firstImageUrl = null;

            for (int i = 0; i < imageFiles.Count; i++)
            {
                var url = $"/hinhanhbienthe/{folderName}/{Path.GetFileName(imageFiles[i])}";
                if (i == 0) firstImageUrl = url;
                _context.HinhAnhBienThes.Add(new HinhAnhBienThe { MaBienThe = maBienThe, ImageUrl = url, LaAnhChinh = (i == 0), ThuTuHienThi = i + 1 });
            }
            await _context.SaveChangesAsync();
            return firstImageUrl;
        }

        // GET: Admin/BienTheSanPhams/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var bienTheSanPham = await _context.BienTheSanPhams.Include(b => b.MaSpNavigation).FirstOrDefaultAsync(m => m.MaBienThe == id);
            return bienTheSanPham == null ? NotFound() : View(bienTheSanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bienTheSanPham = await _context.BienTheSanPhams.FindAsync(id);
            if (bienTheSanPham != null)
            {
                // Xóa ảnh
                var gallery = _context.HinhAnhBienThes.Where(h => h.MaBienThe == id);
                _context.HinhAnhBienThes.RemoveRange(gallery);

                // Xóa tồn kho
                var kho = _context.Khos.Where(k => k.MaBienThe == id);
                _context.Khos.RemoveRange(kho);

                _context.BienTheSanPhams.Remove(bienTheSanPham);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public IActionResult GetImagesInFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return Json(new List<string>());

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe", folderName);
            if (!Directory.Exists(path)) return Json(new List<string>());

            // Lấy TẤT CẢ ảnh trong folder (kể cả folder con nếu có)
            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(f => allowedExt.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => f.Replace(path, "").TrimStart('\\', '/').Replace("\\", "/"))
                .OrderBy(f => f)
                .ToList();

            return Json(files);
        }

        private void LoadFolderList()
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hinhanhbienthe");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            ViewBag.FolderList = Directory.GetDirectories(rootPath).Select(Path.GetFileName).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int maBienThe, int imageId)
        {
            var images = await _context.HinhAnhBienThes.Where(x => x.MaBienThe == maBienThe).ToListAsync();
            foreach (var img in images) { img.LaAnhChinh = (img.IdHinhAnh == imageId); }

            var mainImg = images.FirstOrDefault(x => x.IdHinhAnh == imageId);
            if (mainImg != null)
            {
                var bienthe = await _context.BienTheSanPhams.FindAsync(maBienThe);
                if (bienthe != null) bienthe.ImageUrl = mainImg.ImageUrl;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateImageOrder([FromBody] List<int> imageIds)
        {
            for (int i = 0; i < imageIds.Count; i++)
            {
                var img = await _context.HinhAnhBienThes.FindAsync(imageIds[i]);
                if (img != null) img.ThuTuHienThi = i + 1;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool BienTheSanPhamExists(int id) => _context.BienTheSanPhams.Any(e => e.MaBienThe == id);


        [HttpGet]
        public async Task<IActionResult> GetBienTheBySpId(int maSp)
        {
            var data = await _context.BienTheSanPhams
                .Where(b => b.MaSp == maSp)
                .Select(b => new {
                    b.MaBienThe,
                    b.MaSku,
                    b.MauSac,
                    b.DuongKinhMat,
                    b.ChatLieuDay,
                    b.GiaBan,
                    b.ImageUrl,
                    b.IsActive
                })
                .ToListAsync();

            return Json(data);
        }
    }
}