using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class QuanLyDonHangController : Controller
    {

        private readonly ShopDongHoDbContext _context; 
        public QuanLyDonHangController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "", string searchString = "", DateTime? filterDate = null, bool showAll = false)
        {
            var query = _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ThanhToan)
                .AsQueryable();

            if (!showAll)
            {
                DateTime dateToFilter = filterDate ?? DateTime.Today;
                query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Date == dateToFilter.Date);
                ViewBag.SelectedDate = dateToFilter.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.SelectedDate = "";
            }

          
            var queryThongKe = query.Where(d => d.TrangThai.Trim() == "Hoàn thành" || d.TrangThai.Trim() == "Đã giao hàng");

            decimal tongCoShip = await queryThongKe.SumAsync(d => (decimal?)d.TongTien) ?? 0m;
            decimal tongPhiShip = await queryThongKe.SumAsync(d => (decimal?)d.PhiVanChuyen) ?? 0m;
            decimal tongVoucher = await queryThongKe.SumAsync(d => (decimal?)d.TienGiamGia) ?? 0m;
            decimal tongKhongShip = tongCoShip - tongPhiShip;

            ViewBag.TongDoanhThuKhongShip = tongKhongShip;
            ViewBag.TongDoanhThuCoShip = tongCoShip;
            ViewBag.TongPhiShip = tongPhiShip;
            ViewBag.TongVoucher = tongVoucher;

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.TrangThai.Trim() == status.Trim());
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.MaDonHang.ToString().Contains(searchString)
                                      || d.MaNguoiDungNavigation.HoTen.Contains(searchString));
            }

            var result = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.SearchString = searchString;
            ViewBag.ShowAll = showAll;

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var donhang = await _context.DonHangs.FindAsync(id);

            if (donhang != null && (donhang.TrangThai.Trim() == "Chờ xử lý" || donhang.TrangThai.Trim() == "Chờ xác nhận"))
            {
                donhang.TrangThai = "Đang giao hàng";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Duyệt đơn hàng thành công!";
            }
            else
            {
                TempData["Error"] = "Trạng thái đơn hàng không hợp lệ để duyệt!";
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.MaDiaChiNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaBienTheNavigation)
                .ThenInclude(bt => bt.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
            if (donHang == null) return NotFound();
            return View(donHang); 

        }

        public async Task<IActionResult> PrintLabel(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.MaDiaChiNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ThanhToan)   
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaBienTheNavigation)
                        .ThenInclude(bt => bt.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null) return NotFound();

            // Tính tiền thu hộ: COD thì thu đủ, online đã thanh toán thì thu 0đ
            bool daThanhToanOnline = donHang.ThanhToan?.TrangThai == "Thành công";
            ViewBag.TienThuHo = daThanhToanOnline ? 0 : donHang.TongTien;

            return View(donHang);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var donhang = await _context.DonHangs.FindAsync(id);
            if (donhang != null && donhang.TrangThai == "Đang giao hàng")
            {
                donhang.TrangThai = "Đã giao hàng"; 
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật trạng thái: Đã giao hàng thành công.";
            }
            return RedirectToAction(nameof(Index), new { status = "Đang giao hàng" });
        }

    }
}
