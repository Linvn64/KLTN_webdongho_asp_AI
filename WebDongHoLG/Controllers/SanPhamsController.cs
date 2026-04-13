    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using WebDongHoLG.Data;
    using WebDongHoLG.ViewModels;

    namespace WebDongHoLG.Controllers
    {
        public class SanPhamsController : Controller
        {
            private readonly ShopDongHoDbContext _context;
            private const int PAGE_SIZE = 12;
            private readonly UserManager<IdentityUser> _userManager;

            public SanPhamsController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }


            public async Task<IActionResult> Index(int? danhmuc, int? thuonghieu, string? keyword, string? doiTuong, string? sapXep, string? locMacDinh, int page = 1)
            {
                // BƯỚC 1: DỰNG CÂU QUERY (CHƯA GỌI DATABASE)
                var query = _context.SanPhams
                    .Include(s => s.IdDanhMucNavigation)
                    .Include(s => s.ThuongHieuNavigation)
                    .Include(s => s.BienTheSanPhams)
                        .ThenInclude(bt => bt.Khos)
                    .Include(s => s.BienTheSanPhams)         
                        .ThenInclude(bt => bt.ChiTietDonHangs)
                    .Where(s => s.IsActive == true)
                    .AsQueryable();

                // BƯỚC 2: LỌC DỮ LIỆU BẰNG SQL
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
                        // 1. Tìm trong Tên sản phẩm
                        s.TenSanPham.ToLower().Contains(keyword) ||

                        // 2. TÌM TRONG TÊN THƯƠNG HIỆU 
                        s.ThuongHieuNavigation.TenThuongHieu.ToLower().Contains(keyword) ||

                        // 3. Tìm trong Màu sắc của biến thể 
                        s.BienTheSanPhams.Any(bt => bt.MauSac.ToLower().Contains(keyword))
                    );
                }

                var sanPhams = await query.ToListAsync();

                // BƯỚC 3: MAPPING VÀ GOM NHÓM BIẾN THỂ
                var allVMs = sanPhams.Select(s => {
                    var bienThes = s.BienTheSanPhams.Where(bt => bt.IsActive == true).ToList();
                    var luotBan = bienThes.Sum(bt =>
                        bt.ChiTietDonHangs.Sum(ct => ct.SoLuong ?? 0) // ⚠️ kiểm tra tên field SoLuong
                    );
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
                        ConHang = bienThes.Any(bt => bt.Khos.Sum(k => k.SoLuongTon ?? 0) > 0),
                        LuotBan = luotBan,  
                        NgayTao = s.NgayTao  
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
                        DanhSachTheoDoiTuong = g.OrderBy(s => s.DoiTuong).ToList(),
                        TongLuotBan = g.Sum(s => s.LuotBan),
                        NgayTao = g.Max(s => s.NgayTao)  
                    }).ToList();    

                // BƯỚC 4: THUẬT TOÁN SẮP XẾP ĐA CHIỀU
                IEnumerable <SanPhamNhomVM> ordered = nhomSanPhams;

                if (!string.IsNullOrEmpty(locMacDinh))
                {
                    ordered = locMacDinh switch
                    {
                        "ban-chay" => ordered.OrderByDescending(s => s.TongLuotBan),
                        "moi-nhat" => ordered.OrderByDescending(s => s.NgayTao),
                        _ => ordered.OrderBy(s => s.TenSp)
                    };
                }

                if (!string.IsNullOrEmpty(sapXep))
                {
                    if (ordered is IOrderedEnumerable<SanPhamNhomVM> tempOrdered && !string.IsNullOrEmpty(locMacDinh))
                    {
                        ordered = sapXep switch
                        {
                            "gia-tang" => tempOrdered.ThenBy(s => s.GiaBanThapNhat),
                            "gia-giam" => tempOrdered.ThenByDescending(s => s.GiaBanThapNhat),
                            "ten-az" => tempOrdered.ThenBy(s => s.TenSp),
                            _ => tempOrdered
                        };
                    }
                    else
                    {
                        ordered = sapXep switch
                        {
                            "gia-tang" => ordered.OrderBy(s => s.GiaBanThapNhat),
                            "gia-giam" => ordered.OrderByDescending(s => s.GiaBanThapNhat),
                            "ten-az" => ordered.OrderBy(s => s.TenSp),
                            _ => ordered.OrderBy(s => s.TenSp)
                        };
                    }
                }

                if (string.IsNullOrEmpty(locMacDinh) && string.IsNullOrEmpty(sapXep))
                {
                    ordered = ordered.OrderBy(s => s.TenSp);
                }

                // BƯỚC 5: PHÂN TRANG (PAGINATION) VÀ TRẢ VỀ VIEW
      
                int totalItems = ordered.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)PAGE_SIZE);

                var data = ordered.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

                ViewBag.ThuongHieus = await _context.ThuongHieus.ToListAsync();
                ViewBag.CurrentDanhMuc = danhmuc;
                ViewBag.CurrentThuongHieu = thuonghieu;
                ViewBag.CurrentKeyword = keyword;
                ViewBag.CurrentDoiTuong = doiTuong;
                ViewBag.CurrentSapXep = sapXep;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentLoc = locMacDinh;

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
                    .Include(s => s.DanhGia)
                        .ThenInclude(dg => dg.MaBienTheNavigation)
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

                    DanhGias = sp.DanhGia.OrderByDescending(dg => dg.NgayDanhGia)
                        .Select(dg => new DanhGiaVM
                        {
                            TenNguoiDung = dg.MaNguoiDungNavigation?.HoTen ?? "Người dùng",
                            SoSao = dg.SoSao ?? 5,
                            NoiDung = dg.NoiDung,
                            NgayDanhGia = dg.NgayDanhGia ?? DateTime.Now,
                            TenBienThe = dg.MaBienTheNavigation != null
                                ? $"{dg.MaBienTheNavigation.MauSac} - {dg.MaBienTheNavigation.DuongKinhMat}mm"
                                : ""
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

                var userId = _userManager.GetUserId(User);
                var nguoiDung = userId != null
                    ? await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId)
                    : null;

                if (nguoiDung != null)
                {
                    bool daMua = await _context.DonHangs
                        .Where(d => d.MaNguoiDung == nguoiDung.MaNguoiDung && d.TrangThai.Contains("Hoàn thành"))
                        .SelectMany(d => d.ChiTietDonHangs)
                        .AnyAsync(ct => ct.MaBienTheNavigation.MaSp == id);

                    bool daDanhGia = await _context.DanhGia
                        .AnyAsync(dg => dg.MaNguoiDung == nguoiDung.MaNguoiDung && dg.MaSp == id);

                    ViewBag.CoTheDanhGia = daMua && !daDanhGia;
                    ViewBag.DaDanhGia = daDanhGia;

                    var donHangHoanThanh = await _context.DonHangs
                        .Include(d => d.ChiTietDonHangs)
                            .ThenInclude(ct => ct.MaBienTheNavigation)
                        .Where(d => d.MaNguoiDung == nguoiDung.MaNguoiDung && d.TrangThai.Contains("Hoàn thành"))
                        .OrderByDescending(d => d.NgayDat)
                        .FirstOrDefaultAsync(d => d.ChiTietDonHangs
                            .Any(ct => ct.MaBienTheNavigation.MaSp == id));

                    ViewBag.MaDonHangHoanThanh = donHangHoanThanh?.MaDonHang;
                }

                return View(vm);
            }

            public IActionResult Search(string keyword, int? thuonghieu)
            {
                return RedirectToAction(nameof(Index), new { keyword = keyword, thuonghieu = thuonghieu });
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