using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Controllers
{
    [Authorize]
    public class DonHangKhachController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DonHangKhachController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string status = "", string searchString = "")
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var thoiGianHanDinh = DateTime.Now.AddMinutes(-15);
            var donHangHetHan = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.MaNguoiDung == user.MaNguoiDung
                         && d.TrangThai == "Chờ thanh toán"
                         && d.NgayDat < thoiGianHanDinh)
                .ToListAsync();

            if (donHangHetHan.Any())
            {
                foreach (var don in donHangHetHan)
                {
                    don.TrangThai = "Đã hủy";
                    foreach (var ct in don.ChiTietDonHangs)
                    {
                        var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == ct.MaBienThe);
                        if (kho != null) kho.SoLuongTon += ct.SoLuong;
                    }
                }
                await _context.SaveChangesAsync();
            }

            var query = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(c => c.MaBienTheNavigation)
                .ThenInclude(bt => bt.MaSpNavigation)
                
                .Where(d => d.MaNguoiDung == user.MaNguoiDung)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Chờ xác nhận")
                {
                    query = query.Where(d => d.TrangThai.Trim() == "Đã thanh toán" || d.TrangThai.Trim() == "Chờ xác nhận");
                }
                else if (status == "Đang giao hàng")
                {
                    query = query.Where(d => d.TrangThai.Trim() == "Đang giao hàng" || d.TrangThai.Trim() == "Đã giao hàng");
                }
                else if (status == "Đã hủy")
                {
                    query = query.Where(d => d.TrangThai.Trim().StartsWith("Đã hủy"));
                }
                else
                {
                    query = query.Where(d => d.TrangThai.Trim() == status.Trim());
                }
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                bool isNumeric = int.TryParse(searchString, out int maDonTimKiem);
                query = query.Where(d =>
                    (isNumeric && d.MaDonHang == maDonTimKiem)
                    || d.ChiTietDonHangs.Any(ct => ct.MaBienTheNavigation.MaSpNavigation.TenSanPham.ToLower().Contains(searchString))
                );
            }

            query = query.OrderByDescending(d => d.NgayDat);

            var danhSachDonHang = await query.ToListAsync();

            ViewBag.DanhSachDaDanhGia = await _context.DanhGia
                .Where(dg => dg.MaNguoiDung == user.MaNguoiDung)
                .Select(dg => dg.MaSp + "_" + (dg.MaBienThe ?? 0) + "_" + (dg.MaDonHang ?? 0))
                .ToListAsync();



            ViewBag.CurrentStatus = status;
            ViewBag.SearchString = searchString;

            return View(danhSachDonHang);
        }
        [HttpGet]
        public async Task<IActionResult> HuyDon(int id, string lyDo)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung);

            if (donHang == null) return RedirectToAction("Index");

            if (donHang.TrangThai == "Chờ thanh toán" || donHang.TrangThai == "Chờ xử lý")
            {
                donHang.TrangThai = $"Đã hủy ({lyDo})";

                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == ct.MaBienThe);
                    if (kho != null)
                    {
                        kho.SoLuongTon = (kho.SoLuongTon ?? 0) + (ct.SoLuong ?? 0);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["ToastError"] = "Đơn hàng này không thể hủy!";
            }

            return RedirectToAction("Index", new { status = "Đã hủy" });
        }

        public async Task<IActionResult> DaNhanHang(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);

            if (donHang != null && (donHang.TrangThai.Trim() == "Đang giao hàng" || donHang.TrangThai.Trim() == "Đã giao hàng"))
            {
                donHang.TrangThai = "Hoàn thành";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { status = "Hoàn thành" });
            }

            TempData["Error"] = "Trạng thái đơn hàng không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .Include(d => d.ThanhToan)
                .Include(d => d.MaDiaChiNavigation)
                .Include(d => d.MaVoucherNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaBienTheNavigation)
                .ThenInclude(bt => bt.MaSpNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung);

            if (donHang == null)
            {
                TempData["ToastError"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem";
                return RedirectToAction("Index");
            }
            return View(donHang);
        }
    }
}
