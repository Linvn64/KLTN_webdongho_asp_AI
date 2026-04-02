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
    public class VouchersController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public VouchersController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Vouchers
        // 1. HÀM INDEX CHÍNH
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string status, string searchName)
        {
            // TRUYỀN THÊM tuNgay VÀ denNgay VÀO HÀM LỌC DANH SÁCH
            var vouchers = await GetFilteredVouchers(tuNgay, denNgay, status, searchName);

            // Lấy ra danh sách ID của các Voucher đang hiển thị trên bảng
            var currentVoucherIds = vouchers.Select(v => v.MaVoucher).ToList();

            // Tính toán thống kê ngân sách
            await CalculateVoucherStatistics(tuNgay, denNgay, currentVoucherIds);

            // Gửi các giá trị lọc ngược lại View 
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status;
            ViewBag.SearchName = searchName;

            return View(vouchers);
        }

        // 2. HÀM LỌC DANH SÁCH (Cập nhật thêm tham số thời gian)
        private async Task<List<Voucher>> GetFilteredVouchers(DateTime? tuNgay, DateTime? denNgay, string status, string searchName)
        {
            var query = _context.Vouchers.AsQueryable();

            // --- MỚI: LỌC THEO THỜI GIAN TẠO/BẮT ĐẦU VOUCHER ---
            if (tuNgay.HasValue)
            {
                query = query.Where(v => v.NgayBatDau >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                // Bao gồm đến 23:59:59 của ngày được chọn
                query = query.Where(v => v.NgayBatDau <= denNgay.Value.AddDays(1).AddTicks(-1));
            }
            // --------------------------------------------------

            // Lọc theo tên (Mã Voucher)
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(v => v.TenVoucher.Contains(searchName.ToUpper()));
            }

            // Lọc theo trạng thái hoạt động phức hợp
            if (!string.IsNullOrEmpty(status))
            {
                DateTime now = DateTime.Now;

                if (status == "active")
                {
                    query = query.Where(v =>
                        v.IsActive == true &&
                        v.NgayKetThuc >= now &&
                        (v.SoLuong == null || v.SoLuong == 0 || v.DaDung < v.SoLuong)
                    );
                }
                else if (status == "inactive")
                {
                    query = query.Where(v =>
                        v.IsActive == false ||
                        v.IsActive == null ||
                        v.NgayKetThuc < now ||
                        (v.SoLuong > 0 && v.DaDung >= v.SoLuong)
                    );
                }
            }

            return await query.OrderByDescending(v => v.NgayBatDau).ToListAsync();
        }
        // 3. HÀM PHỤ: TÍNH TOÁN NGÂN SÁCH ĐÃ TIÊU
        private async Task CalculateVoucherStatistics(DateTime? tuNgay, DateTime? denNgay, List<int> currentVoucherIds)
        {
            var queryDonHang = _context.DonHangs
                 .Where(d => d.MaVoucher != null
                 && currentVoucherIds.Contains(d.MaVoucher.Value)
                 && d.TrangThai != "Đã hủy");

            // Lọc theo ngày đặt hàng
            if (tuNgay.HasValue)
                queryDonHang = queryDonHang.Where(d => d.NgayDat >= tuNgay.Value);

            if (denNgay.HasValue)
                queryDonHang = queryDonHang.Where(d => d.NgayDat <= denNgay.Value.AddDays(1).AddTicks(-1));

            // Thống kê chi tiết từng mã
            var thongKeVoucher = await queryDonHang
                .GroupBy(d => d.MaVoucher)
                .Select(g => new { MaVoucher = g.Key, TongTienDaGiam = g.Sum(d => d.TienGiamGia) })
                .ToDictionaryAsync(x => x.MaVoucher, x => x.TongTienDaGiam);

            // Tổng ngân sách toàn bộ các mã đang hiển thị
            decimal tongNganSach = await queryDonHang.SumAsync(d => d.TienGiamGia);

            ViewBag.ThongKeVoucher = thongKeVoucher;
            ViewBag.TongNganSachVoucher = tongNganSach;
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // GET: Admin/Vouchers/Create
        public IActionResult Create()
        {
            var model = new Voucher
            {
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(7),
                GiaTriToiThieu = 0,
                SoLuong = 100
            };
            return View(model);
        }

        // POST: Admin/Vouchers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaVoucher,TenVoucher,PhanTramGiam,GiaTriToiThieu,NgayBatDau,NgayKetThuc,SoLuong,IsActive")] Voucher voucher)
        {
            bool isDuplicate = await _context.Vouchers.AnyAsync(v =>
                                                                v.TenVoucher.ToUpper() == voucher.TenVoucher.ToUpper() &&
                                                                v.NgayBatDau == voucher.NgayBatDau &&
                                                                v.NgayKetThuc == voucher.NgayKetThuc);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Voucher với mã và thời gian này đã tồn tại!");
            }

            if (voucher.NgayKetThuc <= voucher.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải lớn hơn ngày bắt đầu!");
            }

            if (ModelState.IsValid)
            {
                voucher.DaDung = 0;
                voucher.TenVoucher = voucher.TenVoucher.ToUpper().Trim();
                voucher.PhanTramGiam = voucher.PhanTramGiam / 100.0;
                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Vouchers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            // --- CHỐT CHẶN BẢO MẬT ---
            if (voucher.DaDung > 0)
            {
                TempData["ToastError"] = "Voucher này đã phát sinh giao dịch, không thể chỉnh sửa để bảo toàn dữ liệu lịch sử!";
                return RedirectToAction(nameof(Index));
            }
            // -------------------------

            voucher.PhanTramGiam = voucher.PhanTramGiam * 100;
            return View(voucher);
        }
        // POST: Admin/Vouchers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaVoucher,TenVoucher,PhanTramGiam,GiaTriToiThieu,NgayBatDau,NgayKetThuc,IsActive")] Voucher voucher, int SoLuongTangThem, int SoLuongCu)
        {
            if (id != voucher.MaVoucher) return NotFound();

            // Bên trong hàm Edit [HttpPost], ngay sau dòng kiểm tra (id != voucher.MaVoucher) em thêm:
            var currentData = await _context.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.MaVoucher == id);
            if (currentData.DaDung > 0)
            {
                TempData["ToastError"] = "Thao tác thất bại! Voucher đã được sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    voucher.DaDung = currentData.DaDung;

                    voucher.SoLuong = SoLuongCu + SoLuongTangThem;

                        voucher.TenVoucher = voucher.TenVoucher.ToUpper().Trim();
                    voucher.PhanTramGiam = voucher.PhanTramGiam / 100.0;

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.MaVoucher)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }
        // GET: Admin/Vouchers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            if (voucher.DaDung > 0)
            {
                TempData["ToastError"] = "Không thể xóa Voucher đã có người sử dụng. Hãy dùng chức năng Ngừng hoạt động (Tắt)!";
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // POST: Admin/Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                if (voucher.DaDung > 0)
                {
                    TempData["ToastError"] = "Cố tình xóa thất bại! Voucher đã phát sinh giao dịch.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Vouchers.Remove(voucher); // Lệnh này sẽ xóa hẳn khỏi DB
                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Đã xóa vĩnh viễn Voucher khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.MaVoucher == id);
        }

        public async Task<IActionResult> ToggleStatus(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            voucher.IsActive = !(voucher.IsActive ?? false);

            _context.Update(voucher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
