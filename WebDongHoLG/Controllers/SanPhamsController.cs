using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.ViewModels;

namespace WebDongHoLG.Controllers
{
    public class SanPhamsController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private const int PAGE_SIZE = 9;

        public SanPhamsController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? danhmuc, int? thuonghieu,
     string? keyword, string? doiTuong, string? sapXep, int page = 1)
        {
            var query = _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.ThuongHieuNavigation)
                .Include(s => s.BienTheSanPhams)
                    .ThenInclude(bt => bt.Khos)
                .Where(s => s.IsActive == true)
                .AsQueryable();

            if (danhmuc.HasValue)
                query = query.Where(s => s.IdDanhMuc == danhmuc);

            if (thuonghieu.HasValue)
                query = query.Where(s => s.ThuongHieuId == thuonghieu);

            if (!string.IsNullOrEmpty(doiTuong))
                query = query.Where(s => s.DoiTuong == doiTuong);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(s =>
                    s.TenSanPham.ToLower().Contains(keyword) ||
                    s.BienTheSanPhams.Any(bt => bt.MauSac.ToLower().Contains(keyword)));
            }

            var sanPhams = await query.ToListAsync();

            var allVMs = sanPhams.Select(s => {
                var bienThes = s.BienTheSanPhams.Where(bt => bt.IsActive == true).ToList();
                return new SanPhamVM
                {
                    MaSp = s.MaSp,
                    TenSp = s.TenSanPham,
                    Hinh = bienThes.FirstOrDefault()?.ImageUrl,
                    MoTa = s.MoTa,
                    GiaBan = bienThes.Any() ? bienThes.Min(bt => bt.GiaBan) : null,
                    GiaCao = bienThes.Any() ? bienThes.Max(bt => bt.GiaBan) : null,
                    TenDanhMuc = s.IdDanhMucNavigation?.TenDanhMuc,
                    TenThuongHieu = s.ThuongHieuNavigation?.TenThuongHieu,
                    DoiTuong = s.DoiTuong,
                    SoBienThe = bienThes.Count,
                    ConHang = bienThes.Any(bt => bt.Khos.Sum(k => k.SoLuongTon ?? 0) > 0)
                };
            }).ToList();

            var nhomSanPhams = allVMs
                .GroupBy(s => new { s.TenSp, s.TenThuongHieu })
                .Select(g => new SanPhamNhomVM
                {
                    TenSp = g.Key.TenSp,
                    TenThuongHieu = g.Key.TenThuongHieu,
                    Hinh = g.First().Hinh,
                    MoTa = g.First().MoTa,
                    GiaBanThapNhat = g.Min(s => s.GiaBan),
                    TenDanhMuc = g.First().TenDanhMuc,
                    ConHang = g.Any(s => s.ConHang),
                    MaSpDaiDien = g.First().MaSp,
                    DanhSachTheoDoiTuong = g.OrderBy(s => s.DoiTuong).ToList()
                })
                .AsQueryable();

            nhomSanPhams = sapXep switch
            {
                "gia-tang" => nhomSanPhams.OrderBy(s => s.GiaBanThapNhat),
                "gia-giam" => nhomSanPhams.OrderByDescending(s => s.GiaBanThapNhat),
                "ten-az" => nhomSanPhams.OrderBy(s => s.TenSp),
                _ => nhomSanPhams.OrderBy(s => s.TenSp)
            };

            int totalItems = nhomSanPhams.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);
            var data = nhomSanPhams.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.ThuongHieus = await _context.ThuongHieus.ToListAsync();
            ViewBag.CurrentDanhMuc = danhmuc;
            ViewBag.CurrentThuongHieu = thuonghieu;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentDoiTuong = doiTuong;
            ViewBag.CurrentSapXep = sapXep;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(data);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var sp = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.ThuongHieuNavigation)
                .Include(s => s.BienTheSanPhams)
                    .ThenInclude(bt => bt.HinhAnhBienThes)
                .Include(s => s.BienTheSanPhams)
                    .ThenInclude(bt => bt.Khos)
                .Include(s => s.DanhGia)
                    .ThenInclude(dg => dg.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(s => s.MaSp == id);

            if (sp == null) return NotFound();

            var spCungTen = await _context.SanPhams
                .Where(s => s.TenSanPham == sp.TenSanPham
                         && s.ThuongHieuId == sp.ThuongHieuId
                         && s.IsActive == true)
                .OrderBy(s => s.DoiTuong)
                .ToListAsync();

            var vm = new ChiTietSanPhamVM
            {
                MaSp = sp.MaSp,
                TenSp = sp.TenSanPham,
                MoTa = sp.MoTa,
                TenDanhMuc = sp.IdDanhMucNavigation?.TenDanhMuc,
                TenThuongHieu = sp.ThuongHieuNavigation?.TenThuongHieu,
                DoiTuong = sp.DoiTuong,
                SoSaoTrungBinh = sp.DanhGia.Any() ? sp.DanhGia.Average(dg => dg.SoSao ?? 0) : 0,
                SoDanhGia = sp.DanhGia.Count,
                DanhGias = sp.DanhGia.OrderByDescending(dg => dg.NgayDanhGia).Take(5)
                    .Select(dg => new DanhGiaVM
                    {
                        TenNguoiDung = dg.MaNguoiDungNavigation?.HoTen ?? "Ẩn danh",
                        SoSao = dg.SoSao ?? 0,
                        NoiDung = dg.NoiDung,
                        NgayDanhGia = dg.NgayDanhGia ?? DateTime.Now
                    }).ToList(),
                BienThes = sp.BienTheSanPhams.Where(bt => bt.IsActive == true)
                    .Select(bt => new BienTheVM
                    {
                        MaBienThe = bt.MaBienThe,
                        MaSku = bt.MaSku,
                        MauSac = bt.MauSac,
                        DuongKinhMat = bt.DuongKinhMat,
                        ChatLieuDay = bt.ChatLieuDay,
                        GiaBan = bt.GiaBan,
                        GiaNhap = bt.GiaNhap,
                        ImageUrl = bt.ImageUrl,
                        SoLuongTon = bt.Khos.Sum(k => k.SoLuongTon ?? 0),
                        HinhAnhs = bt.HinhAnhBienThes
                            .OrderBy(h => h.ThuTuHienThi)
                            .Select(h => h.ImageUrl)
                            .ToList()
                    }).ToList(),

                CacDoiTuong = spCungTen.Select(s => new DoiTuongVM
                {
                    MaSp = s.MaSp,
                    DoiTuong = s.DoiTuong,
                    DangChon = s.MaSp == id
                }).ToList()
            };

            ViewBag.SpLienQuan = await _context.SanPhams
                .Include(s => s.BienTheSanPhams)
                .Where(s => s.IdDanhMuc == sp.IdDanhMuc
                         && s.MaSp != id
                         && s.TenSanPham != sp.TenSanPham
                         && s.IsActive == true)
                .Take(4)
                .Select(s => new SanPhamVM
                {
                    MaSp = s.MaSp,
                    TenSp = s.TenSanPham,
                    Hinh = s.BienTheSanPhams.FirstOrDefault().ImageUrl,
                    GiaBan = s.BienTheSanPhams.Min(bt => bt.GiaBan),
                    TenDanhMuc = s.IdDanhMucNavigation.TenDanhMuc
                })
                .ToListAsync();

            return View(vm);
        }
        public async Task<IActionResult> Search(string keyword)
        {
            return RedirectToAction(nameof(Index), new { keyword });
        }


        [HttpGet]
        public async Task<IActionResult> GetBienTheChonNhanh(int maSp)
        {
            var sp = await _context.SanPhams
                .Include(s => s.ThuongHieuNavigation)
                .Include(s => s.BienTheSanPhams)
                    .ThenInclude(bt => bt.Khos)
                .FirstOrDefaultAsync(s => s.MaSp == maSp);

            if (sp == null) return NotFound();

            var result = new
            {
                tenSp = sp.TenSanPham,
                tenThuongHieu = sp.ThuongHieuNavigation?.TenThuongHieu,
                bienThes = sp.BienTheSanPhams.Where(bt => bt.IsActive == true).Select(bt => new
                {
                    maBienThe = bt.MaBienThe,
                    mauSac = bt.MauSac,
                    duongKinhMat = bt.DuongKinhMat,
                    chatLieuDay = bt.ChatLieuDay,
                    giaBan = bt.GiaBan,
                    imageUrl = bt.ImageUrl,
                    soLuongTon = bt.Khos.Sum(k => k.SoLuongTon ?? 0),
                    maSku = bt.MaSku
                }).ToList()
            };

            return Json(result);
        }
    }
}