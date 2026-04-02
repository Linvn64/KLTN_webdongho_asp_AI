using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Models;
using WebDongHoLG.Services.Momo;
using WebDongHoLG.Services.Vnpay;
using WebDongHoLG.ViewModels;
using WebDongHoLG.ViewModels.vnpay;

namespace WebDongHoLG.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        public CheckoutController(ShopDongHoDbContext context,
            UserManager<IdentityUser> userManager,
            IVnPayService vnPayService,IMomoService momoService)
        {
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _momoService = momoService;
        }

        private async Task<NguoiDung?> GetCurrentUser()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.NguoiDungs
                .Include(u => u.DiaChiGiaoHangs)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string selectedIds, int? qty, bool isBuyNow = false)
        {
            if (string.IsNullOrEmpty(selectedIds)) return RedirectToAction("Index", "GioHangs");

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = new List<GioHangVM>();

            if (isBuyNow && qty.HasValue)
            {
                int btId = int.Parse(selectedIds);
                var bt = await _context.BienTheSanPhams
                    .Include(b => b.MaSpNavigation).ThenInclude(s => s.ThuongHieuNavigation)
                    .Include(b => b.Khos)
                    .FirstOrDefaultAsync(b => b.MaBienThe == btId);

                if (bt != null)
                    cartItems.Add(new GioHangVM
                    {
                        MaBienThe = bt.MaBienThe,
                        TenSp = bt.MaSpNavigation.TenSanPham,
                        TenThuongHieu = bt.MaSpNavigation.ThuongHieuNavigation?.TenThuongHieu,
                        MauSac = bt.MauSac,
                        DuongKinhMat = bt.DuongKinhMat,
                        ChatLieuDay = bt.ChatLieuDay,
                        MaSku = bt.MaSku,
                        Hinh = bt.ImageUrl,
                        DonGia = bt.GiaBan ?? 0,
                        SoLuong = qty.Value,
                        SoLuongTon = bt.Khos.Sum(k => k.SoLuongTon ?? 0)
                    });
            }
            else
            {
                var ids = selectedIds.Split(',').Select(int.Parse).ToList();
                var gioHang = await _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.MaBienTheNavigation)
                            .ThenInclude(bt => bt.MaSpNavigation)
                                .ThenInclude(sp => sp.ThuongHieuNavigation)
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.MaBienTheNavigation)
                            .ThenInclude(bt => bt.Khos)
                    .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

                if (gioHang != null)
                    cartItems = gioHang.ChiTietGioHangs
                        .Where(c => ids.Contains(c.MaBienThe))
                        .Select(c => new GioHangVM
                        {
                            MaBienThe = c.MaBienThe,
                            TenSp = c.MaBienTheNavigation.MaSpNavigation.TenSanPham,
                            TenThuongHieu = c.MaBienTheNavigation.MaSpNavigation.ThuongHieuNavigation?.TenThuongHieu,
                            MauSac = c.MaBienTheNavigation.MauSac,
                            DuongKinhMat = c.MaBienTheNavigation.DuongKinhMat,
                            ChatLieuDay = c.MaBienTheNavigation.ChatLieuDay,
                            MaSku = c.MaBienTheNavigation.MaSku,
                            Hinh = c.MaBienTheNavigation.ImageUrl,
                            DonGia = c.MaBienTheNavigation.GiaBan ?? 0,
                            SoLuong = c.SoLuong ?? 1,
                            SoLuongTon = c.MaBienTheNavigation.Khos.Sum(k => k.SoLuongTon ?? 0)
                        }).ToList();
            }

            if (!cartItems.Any()) return RedirectToAction("Index", "GioHangs");

            var model = await BuildDonHangVM(user, cartItems);
            ViewBag.IsBuyNow = isBuyNow;
            return View(model);
        }

        // ĐẶT HÀNG
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(DonHangVM model, string selectedIds,
            string CachThanhToan, bool isBuyNow = false)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(selectedIds)) return RedirectToAction("Index", "GioHangs");

            var ids = selectedIds.Split(',').Select(int.Parse).ToList();
            double tongTienHang = 0;
            var chiTietList = new List<ChiTietDonHang>();

            if (isBuyNow)
            {
                foreach (var btId in ids)
                {
                    var bt = await _context.BienTheSanPhams.FindAsync(btId);
                    if (bt == null) continue;
                    var sl = model.DanhSachSanPham?.FirstOrDefault(x => x.MaBienThe == btId)?.SoLuong ?? 1;
                    tongTienHang += (double)(bt.GiaBan ?? 0) * sl;
                    chiTietList.Add(new ChiTietDonHang
                    {
                        MaBienThe = btId,
                        SoLuong = sl,
                        DonGiaTaiThoiDiem = bt.GiaBan ?? 0
                    });
                    var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == btId);
                    if (kho != null) kho.SoLuongTon = Math.Max(0, (kho.SoLuongTon ?? 0) - sl);
                }
            }
            else
            {
                var gioHang = await _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.MaBienTheNavigation)
                    .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

                if (gioHang != null)
                {
                    var items = gioHang.ChiTietGioHangs.Where(c => ids.Contains(c.MaBienThe)).ToList();
                    foreach (var item in items)
                    {
                        tongTienHang += (double)(item.MaBienTheNavigation.GiaBan ?? 0) * (item.SoLuong ?? 0);
                        chiTietList.Add(new ChiTietDonHang
                        {
                            MaBienThe = item.MaBienThe,
                            SoLuong = item.SoLuong ?? 0,
                            DonGiaTaiThoiDiem = item.MaBienTheNavigation.GiaBan ?? 0
                        });
                        var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == item.MaBienThe);
                        if (kho != null) kho.SoLuongTon = Math.Max(0, (kho.SoLuongTon ?? 0) - (item.SoLuong ?? 0));
                        _context.ChiTietGioHangs.Remove(item);
                    }
                }
            }

            double giamGia = 0;
            if (model.MaVoucherChon.HasValue)
            {
                var v = await _context.Vouchers.FindAsync(model.MaVoucherChon);

                if (v != null && v.IsActive == true && (v.DaDung ?? 0) < (v.SoLuong ?? 0))
                {

                    if (tongTienHang >= (double)v.GiaTriToiThieu)
                    {
                        giamGia = tongTienHang * (double)v.PhanTramGiam;
                        v.DaDung = (v.DaDung ?? 0) + 1;
                    }
                    else
                    {
                        double toiThieu = (double)v.GiaTriToiThieu;
                        TempData["ToastError"] = $"Đơn hàng chưa đủ {toiThieu:N0}đ để áp dụng voucher này!";

                        return RedirectToAction("Index", "GioHangs");
                    }
                }
            }
            var maDiaChi = user.DiaChiGiaoHangs
                .OrderByDescending(d => d.MaDiaChi)
                .Select(d => d.MaDiaChi)
                .FirstOrDefault();

            double phiShip = (tongTienHang >= 5000000) ? 0 : 30000;
            double tongThanhToan = tongTienHang - giamGia + phiShip;

            var donHang = new DonHang
            {
                MaNguoiDung = user.MaNguoiDung,
                NgayDat = DateTime.Now,
                TongTien = (decimal)tongThanhToan,
                PhiVanChuyen = (decimal)phiShip,
                TienGiamGia = (decimal)giamGia,
                TrangThai = CachThanhToan == "COD" ? "Chờ xử lý" : "Chờ thanh toán",
                MaVoucher = model.MaVoucherChon,
                MaDiaChi = maDiaChi
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var ct in chiTietList)
            {
                ct.MaDonHang = donHang.MaDonHang;
                _context.ChiTietDonHangs.Add(ct);
            }
            await _context.SaveChangesAsync();

            var tt = new ThanhToan
            {
                MaDonHang = donHang.MaDonHang,
                PhuongThuc = CachThanhToan,
                ThoiGianThanhToan = DateTime.Now,
                TrangThai = CachThanhToan == "COD" ? "Chờ thu tiền" : "Chờ thanh toán"
            };
            _context.ThanhToans.Add(tt);
            await _context.SaveChangesAsync();

            // Phân nhánh theo phương thức thanh toán
            if (CachThanhToan == "VNPAY")
            {
                HttpContext.Session.SetInt32("PendingOrderId", donHang.MaDonHang);
                var payModel = new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = tongThanhToan,
                    OrderDescription = $"Thanh toan don hang #{donHang.MaDonHang}",
                    Name = user.HoTen ?? "Khách hàng"
                };
                var url = _vnPayService.CreatePaymentUrl(payModel, HttpContext);
                return Redirect(url);
            }

            // Trong PlaceOrder — thêm sau block VNPAY
            if (CachThanhToan == "MOMO")
            {
                HttpContext.Session.SetInt32("PendingOrderId", donHang.MaDonHang);
                var momoModel = new OrderInfoModel
                {
                    OrderId = donHang.MaDonHang.ToString() + "_" + DateTime.Now.Ticks.ToString(),
                    FullName = user.HoTen ?? "Khách hàng",
                    Amount = ((long)tongThanhToan).ToString(),
                    OrderInfo = $"Thanh toan don hang #{donHang.MaDonHang}"
                };
                var momoResponse = await _momoService.CreatePaymentMomo(momoModel);
                if (momoResponse?.PayUrl != null)
                    return Redirect(momoResponse.PayUrl);

                TempData["ToastError"] = "Không thể kết nối Momo!";
                return RedirectToAction("PaymentPending", new { maDonHang = donHang.MaDonHang });
            }

            // COD
            TempData["ToastSuccess"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderSuccess", new { maDonHang = donHang.MaDonHang });
        }

        // KẾT QUẢ ĐẶT HÀNG
        public async Task<IActionResult> OrderSuccess(int maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaBienTheNavigation)
                        .ThenInclude(bt => bt.MaSpNavigation)
                .Include(d => d.MaDiaChiNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null) return RedirectToAction("Index", "Home");
            return View(donHang);
        }

        public async Task<IActionResult> BankTransfer(int maDonHang)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null) return RedirectToAction("Index", "Home");
            ViewBag.MaDonHang = maDonHang;
            ViewBag.TongTien = donHang.TongTien;
            return View();
        }

        
        // ĐỊA CHỈ
        public class AddressRequest
        {
            public int MaDiaChi { get; set; }
            public string HoTen { get; set; } = "";
            public string SdtNhanHang { get; set; } = "";
            public string TinhThanh { get; set; } = "";
            public string QuanHuyen { get; set; } = "";
            public string PhuongXa { get; set; } = "";
            public string DiaChiChiTiet { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressRequest req)
        {
            if (string.IsNullOrEmpty(req.SdtNhanHang) || !req.SdtNhanHang.StartsWith("0") || req.SdtNhanHang.Length < 10)
                return Json(new { success = false, message = "Số điện thoại không hợp lệ!" });

            var user = await GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var parts = new[] { req.DiaChiChiTiet, req.PhuongXa, req.QuanHuyen, req.TinhThanh }
                        .Where(s => !string.IsNullOrEmpty(s));
            string fullAddress = string.Join(", ", parts);

            if (req.MaDiaChi > 0)
            {
                var dc = await _context.DiaChiGiaoHangs.FindAsync(req.MaDiaChi);
                if (dc != null) { dc.SdtNhanHang = req.SdtNhanHang; dc.DiaChi = fullAddress; dc.GhiChu = req.TinhThanh; }
            }
            else
            {
                bool isDuplicate = await _context.DiaChiGiaoHangs
                    .AnyAsync(d => d.MaNguoiDung == user.MaNguoiDung
                                && d.DiaChi.Trim().ToLower() == fullAddress.Trim().ToLower()
                                && d.SdtNhanHang == req.SdtNhanHang);
                if (isDuplicate) return Json(new { success = false, message = "Địa chỉ này đã tồn tại!" });

                _context.DiaChiGiaoHangs.Add(new DiaChiGiaoHang
                {
                    MaNguoiDung = user.MaNguoiDung,
                    DiaChi = fullAddress,
                    SdtNhanHang = req.SdtNhanHang,
                    GhiChu = req.TinhThanh
                });
            }

            user.HoTen = req.HoTen;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int maDc)
        {
            var dc = await _context.DiaChiGiaoHangs.FindAsync(maDc);
            if (dc == null) return Json(new { success = false });
            _context.DiaChiGiaoHangs.Remove(dc);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> SelectAddress(int maDc)
        {
            var user = await GetCurrentUser();
            if (user == null) return Json(new { success = false });

            var dc = user.DiaChiGiaoHangs.FirstOrDefault(d => d.MaDiaChi == maDc);
            if (dc == null) return Json(new { success = false });

            _context.DiaChiGiaoHangs.Remove(dc);
            _context.DiaChiGiaoHangs.Add(new DiaChiGiaoHang
            {
                MaNguoiDung = dc.MaNguoiDung,
                DiaChi = dc.DiaChi,
                SdtNhanHang = dc.SdtNhanHang,
                GhiChu = dc.GhiChu
            });
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // HELPER
        private async Task<DonHangVM> BuildDonHangVM(NguoiDung user, List<GioHangVM> cartItems)
        {
            var dc = user.DiaChiGiaoHangs.OrderByDescending(d => d.MaDiaChi).FirstOrDefault();
            double tongTienHang = (double)cartItems.Sum(x => x.ThanhTien);
            double phiShip = dc != null && dc.GhiChu != "Hà Nội"
                ? Math.Max(30000, Math.Min(300000, tongTienHang * 0.1)) : 0;

            var today = DateTime.Now;
            var dsVoucher = await _context.Vouchers
                .Where(v => v.IsActive == true && v.NgayKetThuc >= today.Date)
                .ToListAsync();
            var vMacDinh = dsVoucher.FirstOrDefault(v =>
                today >= v.NgayBatDau && today <= v.NgayKetThuc && (v.DaDung ?? 0) < (v.SoLuong ?? 0));

            return new DonHangVM
            {
                HoTen = user.HoTen,
                Email = user.Email,
                SdtNhanHang = dc?.SdtNhanHang ?? user.Sdt,
                TinhThanh = dc?.GhiChu ?? "Chưa xác định",
                DiaChiChiTiet = dc?.DiaChi ?? "Chưa có địa chỉ giao hàng",
                DanhSachSanPham = cartItems,
                PhiVanChuyen = phiShip,
                DanhSachDiaChi = user.DiaChiGiaoHangs.OrderByDescending(d => d.MaDiaChi).ToList(),
                DanhSachVoucher = dsVoucher,
                GiamGiaVoucher = vMacDinh != null ? tongTienHang * (double)vMacDinh.PhanTramGiam : 0,
                MaVoucherChon = vMacDinh?.MaVoucher,
                NgayHeThong = today
            };
        }
        // CHỜ THANH TOÁN (khi hủy VNPay)
        public async Task<IActionResult> PaymentPending(int maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaBienTheNavigation)
                        .ThenInclude(bt => bt.MaSpNavigation)
                .Include(d => d.MaDiaChiNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null) return RedirectToAction("Index", "Home");
            return View(donHang);
        }

        [HttpPost]
        public async Task<IActionResult> RetryPayment(int maDonHang, string CachThanhToan)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null) return RedirectToAction("Index", "GioHangs");

            if (donHang.NgayDat.Value.AddMinutes(15) < DateTime.Now)
            {
                if (donHang.TrangThai != "Đã hủy")
                {
                    donHang.TrangThai = "Đã hủy";
                    foreach (var ct in donHang.ChiTietDonHangs)
                    {
                        var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == ct.MaBienThe);
                        if (kho != null) kho.SoLuongTon = (kho.SoLuongTon ?? 0) + (ct.SoLuong ?? 0);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["ToastError"] = "Đơn hàng đã hết hạn thanh toán (quá 15 phút) và đã bị hủy tự động!";
                return RedirectToAction("Index", "DonHangKhach"); // Đá về trang lịch sử
            }

            if (CachThanhToan == "VNPAY")
            {
                donHang.TrangThai = "Chờ thanh toán";
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("PendingOrderId", maDonHang);
                var payModel = new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = (double)donHang.TongTien,
                    OrderDescription = $"Thanh toan don hang #{maDonHang}",
                    Name = user.HoTen ?? "Khách hàng"
                };
                return Redirect(_vnPayService.CreatePaymentUrl(payModel, HttpContext));
            }

            if (CachThanhToan == "MOMO")
            {
                donHang.TrangThai = "Chờ thanh toán";
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("PendingOrderId", maDonHang);
                var momoModel = new OrderInfoModel
                {
                    OrderId = maDonHang.ToString() + "_" + DateTime.Now.Ticks.ToString(),
                    FullName = user.HoTen ?? "Khách hàng",
                    Amount = ((long)donHang.TongTien).ToString(),  
                    OrderInfo = $"Thanh toan don hang #{maDonHang}"
                };
                var momoResponse = await _momoService.CreatePaymentMomo(momoModel);
                if (momoResponse?.PayUrl != null)
                    return Redirect(momoResponse.PayUrl);

                TempData["ToastError"] = "Không thể kết nối Momo!";
                return RedirectToAction("PaymentPending", new { maDonHang });
            }

            // COD
            donHang.TrangThai = "Chờ xử lý";
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderSuccess", new { maDonHang });
        }
    }
}
