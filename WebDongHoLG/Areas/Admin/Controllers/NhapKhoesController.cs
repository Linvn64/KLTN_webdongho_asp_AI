using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]  // ← đặt đây, bên ngoài class
    public class NhapKhoController : Controller  // ← chỉ 1 class duy nhất, bỏ NhapKhoesController
    {
        private readonly ShopDongHoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NhapKhoController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/NhapKho
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? searchString)
        {
            var query = _context.NhapKhos
                .Include(n => n.MaBienTheNavigation)
                    .ThenInclude(bt => bt.MaSpNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(n =>
                    n.MaBienTheNavigation.MaSpNavigation.TenSanPham.ToLower().Contains(searchString) ||
                    n.MaBienTheNavigation.MaSku.ToLower().Contains(searchString) ||
                    n.NguoiNhap.ToLower().Contains(searchString));
            }

            if (tuNgay.HasValue)
                query = query.Where(n => n.NgayNhap >= tuNgay.Value);

            if (denNgay.HasValue)
                query = query.Where(n => n.NgayNhap <= denNgay.Value.AddDays(1));

            var lichSu = await query.OrderByDescending(n => n.NgayNhap).ToListAsync();

            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSearch = searchString;
            ViewBag.TongLanNhap = lichSu.Count;
            ViewBag.TongSoLuong = lichSu.Sum(n => n.SoLuongNhap);
            ViewBag.TongGiaTri = lichSu.Sum(n => n.SoLuongNhap * n.GiaNhapLuuTru);

            return View(lichSu);
        }

        // GET: Admin/NhapKho/Create
        // Gộp 2 Create GET thành 1, nhận maSp optional
        public IActionResult Create(int? maSp)
        {
            ViewData["SanPhams"] = _context.SanPhams
                .Select(s => new {
                    s.MaSp,
                    TenHienThi = s.TenSanPham + (string.IsNullOrEmpty(s.DoiTuong) ? "" : " [" + s.DoiTuong + "]")
                }).ToList();

            ViewBag.SelectedMaSp = maSp;
            return View();
        }

        // POST: Admin/NhapKho/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<int> MaBienThes, List<int> SoLuongNhaps, List<decimal> GiaNhaps)
        {
            if (MaBienThes == null || MaBienThes.Count == 0)
            {
                TempData["Error"] = "Vui lòng thêm ít nhất 1 biến thể!";
                return RedirectToAction(nameof(Create));
            }

            var user = await _userManager.GetUserAsync(User);
            string nguoiNhap = user?.UserName ?? "Admin";
            var ngayNhap = DateTime.Now;
            int tongSoLuong = 0;

            for (int i = 0; i < MaBienThes.Count; i++)
            {
                int maBienThe = MaBienThes[i];
                int soLuong = i < SoLuongNhaps.Count ? SoLuongNhaps[i] : 0;
                decimal giaNhap = i < GiaNhaps.Count ? GiaNhaps[i] : 0;

                if (soLuong <= 0) continue;

                _context.NhapKhos.Add(new NhapKho
                {
                    MaBienThe = maBienThe,
                    SoLuongNhap = soLuong,
                    GiaNhapLuuTru = giaNhap,
                    NgayNhap = ngayNhap,
                    NguoiNhap = nguoiNhap
                });

                var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == maBienThe);
                if (kho != null)
                    kho.SoLuongTon = (kho.SoLuongTon ?? 0) + soLuong;
                else
                    _context.Khos.Add(new Kho { MaBienThe = maBienThe, SoLuongTon = soLuong });

                tongSoLuong += soLuong;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Nhập kho thành công! Đã nhập {tongSoLuong} sản phẩm lúc {ngayNhap:dd/MM/yyyy HH:mm} bởi {nguoiNhap}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetBienThe(int maSp)
        {
            var data = await _context.BienTheSanPhams
                .Where(bt => bt.MaSp == maSp && bt.IsActive == true)
                .Select(bt => new {
                    bt.MaBienThe,
                    bt.MaSku,
                    bt.MauSac,
                    bt.DuongKinhMat,
                    bt.ChatLieuDay,
                    bt.GiaNhap,
                    GiaNhapGanNhat = bt.NhapKhos
                        .OrderByDescending(n => n.NgayNhap)
                        .Select(n => (decimal?)n.GiaNhapLuuTru)
                        .FirstOrDefault(),
                    TonHienTai = bt.Khos
                        .Select(k => k.SoLuongTon)
                        .FirstOrDefault() ?? 0
                })
                        .ToListAsync();

            return Json(data);
        }
       
    }
}