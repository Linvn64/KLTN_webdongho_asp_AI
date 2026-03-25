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
                .OrderByDescending(d => d.NgayDat)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.TrangThai.Trim() == status.Trim());
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                bool isNumric = int.TryParse(searchString, out int maDonTimKiem);
                query = query.Where(d =>
                (isNumric && d.MaDonHang == maDonTimKiem)
                ||
                d.ChiTietDonHangs.Any(ct => ct.MaBienTheNavigation.MaSpNavigation.TenSanPham.ToLower().Contains(searchString))
                ); 
            }

            query = query.OrderByDescending(d => d.NgayDat); 

            var danhSachDonHang = await query.ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.SearchString = searchString; 

            return View(danhSachDonHang);
        }


        [HttpGet]
        public async Task<IActionResult> HuyDon(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung); 
                
            if (donHang == null)
            {
                TempData["ToastError"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index"); 
            }

            if (donHang.TrangThai == "Chờ thanh toán" || donHang.TrangThai == "Chờ xử lý")
            {
                donHang.TrangThai = "Đã hủy"; 
                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == ct.MaBienThe); 


                    if (kho != null)
                    {
                        kho.SoLuongTon = (kho.SoLuongTon ?? 0) + (ct.SoLuong ?? 0); 
                    }
                }

                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Đã hủy đơn hàng #" + id + " thành công!"; 
            }
            else
            {
                TempData["ToastError"] = "Đơn hàng đang được giao, không thể hủy!"; 
            }

            return RedirectToAction("Index", new { status = "Đã hủy" }); 
        }


        [HttpGet]
        public async Task<IActionResult> DaNhanHang(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung);

            if (donHang == null)
            {
                TempData["ToastError"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index");
            }

            if (donHang.TrangThai == "Đang giao hàng")
            {
                donHang.TrangThai = "Hoàn thành";
                await _context.SaveChangesAsync();

                TempData["ToastSuccess"] = "Cảm ơn bạn! Đơn hàng #" + id + " đã hoàn thành.";
            }
            else
            {
                TempData["ToastError"] = "Trạng thái đơn hàng không hợp lệ!";
            }

            return RedirectToAction("Index", new { status = "Hoàn thành" });
        }
    }
}
