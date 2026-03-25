using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Models;
using WebDongHoLG.ViewModels;

namespace WebDongHoLG.Controllers
{
    [Authorize]
    public class GioHangsController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GioHangsController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<NguoiDung?> GetCurrentUser()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var gioHang = await _context.GioHangs
                            .Include(g => g.ChiTietGioHangs)
                            .ThenInclude(c => c.MaBienTheNavigation)
                            .ThenInclude(bt => bt.MaSpNavigation)
                            .ThenInclude(sp => sp.ThuongHieuNavigation)
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(c => c.MaBienTheNavigation)
                        .ThenInclude(bt => bt.Khos)
                .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

            var data = new List<GioHangVM>();
            if (gioHang != null)
            {
                data = gioHang.ChiTietGioHangs.Select(c => new GioHangVM
                {
                    MaSpGoc = c.MaBienTheNavigation.MaSp,
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

            return View(data);
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { ReturnUrl = $"/GioHangs/AddToCart?id={id}&quantity={quantity}" });

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var bienThe = await _context.BienTheSanPhams
                .Include(bt => bt.Khos)
                .Include(bt => bt.MaSpNavigation)
                .FirstOrDefaultAsync(bt => bt.MaBienThe == id);

            if (bienThe == null)
            {
                TempData["ToastError"] = "Sản phẩm không tồn tại!";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            var tonKho = bienThe.Khos.Sum(k => k.SoLuongTon ?? 0);

            if (tonKho <= 0)
            {
                TempData["ToastError"] = $"Rất tiếc! <strong>{bienThe.MaSpNavigation.TenSanPham}</strong> đã hết hàng.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null)
            {
                gioHang = new GioHang { MaNguoiDung = user.MaNguoiDung, NgayTao = DateTime.Now };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang && c.MaBienThe == id);

            int soLuongHienTai = chiTiet?.SoLuong ?? 0;
            int soLuongMoi = soLuongHienTai + quantity;

            if (soLuongMoi > tonKho)
            {
                TempData["ToastWarning"] = $"Tồn kho không đủ! Chỉ còn <strong>{tonKho}</strong> sản phẩm. Giỏ hàng của bạn đang có <strong>{soLuongHienTai}</strong>.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            if (chiTiet == null)
            {
                chiTiet = new ChiTietGioHang { MaGioHang = gioHang.MaGioHang, MaBienThe = id, SoLuong = soLuongMoi };
                _context.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                chiTiet.SoLuong = soLuongMoi;
            }

            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = $"Đã thêm vào giỏ hàng!";
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
        [Authorize]
        public async Task<IActionResult> RemoveCart(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(c => c.MaBienThe == id
                    && c.MaGioHangNavigation.MaNguoiDung == user.MaNguoiDung);

            if (chiTiet != null)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateQuantity([FromBody] GioHangItemVM data)
        {
            var user = await GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var chiTiet = await _context.ChiTietGioHangs
                .Include(c => c.MaBienTheNavigation)
                    .ThenInclude(bt => bt.Khos)
                .FirstOrDefaultAsync(c => c.MaBienThe == data.Id
                    && c.MaGioHangNavigation.MaNguoiDung == user.MaNguoiDung);

            if (chiTiet == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ!" });

            var tonKho = chiTiet.MaBienTheNavigation.Khos.Sum(k => k.SoLuongTon ?? 0);

            if (data.Quantity < 1)
                return Json(new { success = false, message = "Số lượng tối thiểu là 1!" });

            if (data.Quantity > tonKho)
                return Json(new { success = false, message = $"Chỉ còn {tonKho} sản phẩm trong kho!" });

            chiTiet.SoLuong = data.Quantity;
            await _context.SaveChangesAsync();

            var thanhTien = (chiTiet.MaBienTheNavigation.GiaBan ?? 0) * data.Quantity;
            return Json(new { success = true, thanhTien = thanhTien.ToString("#,##0") });
        }

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Checkout(string selectedIds, int? qty, bool isBuyNow = false)
        //{
        //    if (string.IsNullOrEmpty(selectedIds)) return RedirectToAction("Index");

        //    var user = await GetCurrentUser();
        //    if (user == null) return RedirectToAction("Login", "Account");

        //    await _context.Entry(user).Collection(u => u.DiaChiGiaoHangs).LoadAsync();

        //    var cartItems = new List<GioHangVM>();

        //    if (isBuyNow && qty.HasValue)
        //    {
        //        int btId = int.Parse(selectedIds);
        //        var bt = await _context.BienTheSanPhams
        //            .Include(b => b.MaSpNavigation).ThenInclude(s => s.ThuongHieuNavigation)
        //            .Include(b => b.Khos)
        //            .FirstOrDefaultAsync(b => b.MaBienThe == btId);

        //        if (bt != null)
        //        {
        //            cartItems.Add(new GioHangVM
        //            {
        //                MaBienThe = bt.MaBienThe,
        //                TenSp = bt.MaSpNavigation.TenSanPham,
        //                TenThuongHieu = bt.MaSpNavigation.ThuongHieuNavigation?.TenThuongHieu,
        //                MauSac = bt.MauSac,
        //                DuongKinhMat = bt.DuongKinhMat,
        //                ChatLieuDay = bt.ChatLieuDay,
        //                MaSku = bt.MaSku,
        //                Hinh = bt.ImageUrl,
        //                DonGia = bt.GiaBan ?? 0,
        //                SoLuong = qty.Value,
        //                SoLuongTon = bt.Khos.Sum(k => k.SoLuongTon ?? 0)
        //            });
        //        }
        //    }
        //    else
        //    {
        //        var ids = selectedIds.Split(',').Select(int.Parse).ToList();
        //        var gioHang = await _context.GioHangs
        //            .Include(g => g.ChiTietGioHangs)
        //                .ThenInclude(c => c.MaBienTheNavigation)
        //                    .ThenInclude(bt => bt.MaSpNavigation)
        //                        .ThenInclude(sp => sp.ThuongHieuNavigation)
        //            .Include(g => g.ChiTietGioHangs)
        //                .ThenInclude(c => c.MaBienTheNavigation)
        //                    .ThenInclude(bt => bt.Khos)
        //            .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

        //        if (gioHang != null)
        //        {
        //            cartItems = gioHang.ChiTietGioHangs
        //                .Where(c => ids.Contains(c.MaBienThe))
        //                .Select(c => new GioHangVM
        //                {

        //                    MaBienThe = c.MaBienThe,
        //                    TenSp = c.MaBienTheNavigation.MaSpNavigation.TenSanPham,
        //                    TenThuongHieu = c.MaBienTheNavigation.MaSpNavigation.ThuongHieuNavigation?.TenThuongHieu,
        //                    MauSac = c.MaBienTheNavigation.MauSac,
        //                    DuongKinhMat = c.MaBienTheNavigation.DuongKinhMat,
        //                    ChatLieuDay = c.MaBienTheNavigation.ChatLieuDay,
        //                    MaSku = c.MaBienTheNavigation.MaSku,
        //                    Hinh = c.MaBienTheNavigation.ImageUrl,
        //                    DonGia = c.MaBienTheNavigation.GiaBan ?? 0,
        //                    SoLuong = c.SoLuong ?? 1,
        //                    SoLuongTon = c.MaBienTheNavigation.Khos.Sum(k => k.SoLuongTon ?? 0)
        //                }).ToList();
        //        }
        //    }

        //    if (!cartItems.Any()) return RedirectToAction("Index");

        //    double tongTienHang = (double)cartItems.Sum(x => x.ThanhTien);
        //    var dc = user.DiaChiGiaoHangs.OrderByDescending(d => d.MaDiaChi).FirstOrDefault();
        //    double phiShip = dc != null && dc.GhiChu != "Hà Nội"
        //        ? Math.Max(30000, Math.Min(300000, tongTienHang * 0.1)) : 0;

        //    var today = DateTime.Now;
        //    var dsVoucher = await _context.Vouchers
        //        .Where(v => v.IsActive == true && v.NgayKetThuc >= today.Date)
        //        .ToListAsync();
        //    var vMacDinh = dsVoucher.FirstOrDefault(v =>
        //        today >= v.NgayBatDau && today <= v.NgayKetThuc && (v.DaDung ?? 0) < (v.SoLuong ?? 0));

        //    var model = new DonHangVM
        //    {
        //        HoTen = user.HoTen,
        //        Email = user.Email,
        //        SdtNhanHang = dc?.SdtNhanHang ?? user.Sdt,
        //        TinhThanh = dc?.GhiChu ?? "Chưa xác định",
        //        DiaChiChiTiet = dc?.DiaChi ?? "Chưa có địa chỉ giao hàng",
        //        DanhSachSanPham = cartItems,
        //        PhiVanChuyen = phiShip,
        //        DanhSachDiaChi = user.DiaChiGiaoHangs.OrderByDescending(d => d.MaDiaChi).ToList(),
        //        DanhSachVoucher = dsVoucher,
        //        GiamGiaVoucher = vMacDinh != null ? tongTienHang * (double)(vMacDinh.PhanTramGiam) : 0,
        //        MaVoucherChon = vMacDinh?.MaVoucher,
        //        NgayHeThong = today
        //    };

        //    ViewBag.IsBuyNow = isBuyNow;
        //    return View(model);
        //}

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> PlaceOrder(DonHangVM model, string selectedIds, string CachThanhToan, bool isBuyNow = false)
        //{
        //    var user = await GetCurrentUser();
        //    if (user == null) return RedirectToAction("Login", "Account");
        //    if (string.IsNullOrEmpty(selectedIds)) return RedirectToAction("Index");

        //    var ids = selectedIds.Split(',').Select(int.Parse).ToList();
        //    double tongTienHang = 0;
        //    var chiTietDonHang = new List<ChiTietDonHang>();

        //    if (isBuyNow)
        //    {
        //        foreach (var btId in ids)
        //        {
        //            var bt = await _context.BienTheSanPhams.FindAsync(btId);
        //            if (bt == null) continue;
        //            var sl = model.DanhSachSanPham?.FirstOrDefault(x => x.MaBienThe == btId)?.SoLuong ?? 1;
        //            tongTienHang += (double)(bt.GiaBan ?? 0) * sl;
        //            chiTietDonHang.Add(new ChiTietDonHang
        //            {
        //                MaBienThe = btId,
        //                SoLuong = sl,
        //                DonGiaTaiThoiDiem = bt.GiaBan ?? 0
        //            });

        //            var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == btId);
        //            if (kho != null) kho.SoLuongTon = Math.Max(0, (kho.SoLuongTon ?? 0) - sl);
        //        }
        //    }
        //    else
        //    {
        //        var gioHang = await _context.GioHangs
        //            .Include(g => g.ChiTietGioHangs)
        //                .ThenInclude(c => c.MaBienTheNavigation)
        //            .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

        //        if (gioHang != null)
        //        {
        //            var items = gioHang.ChiTietGioHangs.Where(c => ids.Contains(c.MaBienThe)).ToList();
        //            foreach (var item in items)
        //            {
        //                tongTienHang += (double)(item.MaBienTheNavigation.GiaBan ?? 0) * (item.SoLuong ?? 0);
        //                chiTietDonHang.Add(new ChiTietDonHang
        //                {
        //                    MaBienThe = item.MaBienThe,
        //                    SoLuong = item.SoLuong ?? 0,
        //                    DonGiaTaiThoiDiem = item.MaBienTheNavigation.GiaBan ?? 0
        //                });

        //                var kho = await _context.Khos.FirstOrDefaultAsync(k => k.MaBienThe == item.MaBienThe);
        //                if (kho != null) kho.SoLuongTon = Math.Max(0, (kho.SoLuongTon ?? 0) - (item.SoLuong ?? 0));

        //                _context.ChiTietGioHangs.Remove(item);
        //            }
        //        }
        //    }

        //    double giamGia = 0;
        //    if (model.MaVoucherChon.HasValue)
        //    {
        //        var v = await _context.Vouchers.FindAsync(model.MaVoucherChon);
        //        if (v != null && v.IsActive == true && (v.DaDung ?? 0) < (v.SoLuong ?? 0))
        //        {
        //            giamGia = tongTienHang * (double)(v.PhanTramGiam);
        //            v.DaDung = (v.DaDung ?? 0) + 1;
        //        }
        //    }

        //    var maDiaChi = await _context.DiaChiGiaoHangs
        //        .Where(d => d.MaNguoiDung == user.MaNguoiDung)
        //        .OrderByDescending(d => d.MaDiaChi)
        //        .Select(d => d.MaDiaChi)
        //        .FirstOrDefaultAsync();

        //    var donHang = new DonHang
        //    {
        //        MaNguoiDung = user.MaNguoiDung,
        //        NgayDat = DateTime.Now,
        //        TongTien = (decimal)(tongTienHang - giamGia + model.PhiVanChuyen),
        //        TrangThai = "Chờ xử lý",
        //        MaVoucher = model.MaVoucherChon,
        //        MaDiaChi = maDiaChi
        //    };

        //    _context.DonHangs.Add(donHang);
        //    await _context.SaveChangesAsync();

        //    foreach (var ct in chiTietDonHang)
        //    {
        //        ct.MaDonHang = donHang.MaDonHang;
        //        _context.ChiTietDonHangs.Add(ct);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index", "Home");
        //}

        // Thêm class này vào GioHangsController để hứng dữ liệu AJAX
        public class AddressRequest
        {
            public int MaDiaChi { get; set; }
            public string HoTen { get; set; }
            public string SdtNhanHang { get; set; }
            public string TinhThanh { get; set; }
            public string QuanHuyen { get; set; }
            public string PhuongXa { get; set; }
            public string DiaChiChiTiet { get; set; }
        }

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> UpdateAddress([FromBody] AddressRequest req)
        //{
        //    if (string.IsNullOrEmpty(req.SdtNhanHang) || !req.SdtNhanHang.StartsWith("0") || req.SdtNhanHang.Length < 10)
        //        return Json(new { success = false, message = "Số điện thoại không hợp lệ!" });

        //    var user = await GetCurrentUser();
        //    if (user == null) return Json(new { success = false, message = "Chưa đăng nhập" });

        //    var parts = new List<string> { req.DiaChiChiTiet, req.PhuongXa, req.QuanHuyen, req.TinhThanh }
        //                .Where(s => !string.IsNullOrEmpty(s));
        //    string fullAddress = string.Join(", ", parts);
        //    string addressClean = fullAddress.Trim().ToLower();

        //    if (req.MaDiaChi == 0)
        //    {
        //        bool isDuplicate = await _context.DiaChiGiaoHangs
        //            .AnyAsync(d => d.MaNguoiDung == user.MaNguoiDung
        //                        && d.DiaChi.Trim().ToLower() == addressClean
        //                        && d.SdtNhanHang == req.SdtNhanHang);
        //        if (isDuplicate)
        //            return Json(new { success = false, message = "Địa chỉ này đã có trong danh sách của bạn rồi!" });
        //    }

        //    if (req.MaDiaChi > 0)
        //    {
        //        var dc = await _context.DiaChiGiaoHangs.FindAsync(req.MaDiaChi);
        //        if (dc != null)
        //        {
        //            dc.SdtNhanHang = req.SdtNhanHang;
        //            dc.DiaChi = fullAddress;
        //            dc.GhiChu = req.TinhThanh;
        //        }
        //    }
        //    else
        //    {
        //        _context.DiaChiGiaoHangs.Add(new DiaChiGiaoHang
        //        {
        //            MaNguoiDung = user.MaNguoiDung,
        //            DiaChi = fullAddress,
        //            SdtNhanHang = req.SdtNhanHang,
        //            GhiChu = req.TinhThanh
        //        });
        //    }

        //    user.HoTen = req.HoTen; 
        //    await _context.SaveChangesAsync();

        //    return Json(new { success = true });
        //}


        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> DeleteAddress(int maDc)
        //{
        //    var dc = await _context.DiaChiGiaoHangs.FindAsync(maDc);
        //    if (dc != null)
        //    {
        //        _context.DiaChiGiaoHangs.Remove(dc);
        //        await _context.SaveChangesAsync();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}

        public class DoiBienTheVM
        {
            public int MaBienTheCu { get; set; }
            public int MaBienTheMoi { get; set; }
            public int SoLuong { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DoiBienThe([FromBody] DoiBienTheVM data)
        {
            var user = await GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null) return Json(new { success = false, message = "Giỏ hàng trống!" });

            var bienTheMoi = await _context.BienTheSanPhams
                .Include(bt => bt.Khos)
                .FirstOrDefaultAsync(bt => bt.MaBienThe == data.MaBienTheMoi);

            if (bienTheMoi == null) return Json(new { success = false, message = "Biến thể không tồn tại!" });

            var tonKho = bienTheMoi.Khos.Sum(k => k.SoLuongTon ?? 0);
            if (data.SoLuong > tonKho)
                return Json(new { success = false, message = $"Chỉ còn {tonKho} sản phẩm!" });

            var chiTietCu = gioHang.ChiTietGioHangs
                .FirstOrDefault(c => c.MaBienThe == data.MaBienTheCu);
            int soLuongCu = chiTietCu?.SoLuong ?? data.SoLuong;

            var chiTietMoiTonTai = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang
                                        && c.MaBienThe == data.MaBienTheMoi);

            if (chiTietCu != null)
                _context.ChiTietGioHangs.Remove(chiTietCu);

            if (chiTietMoiTonTai != null)
            {
                chiTietMoiTonTai.SoLuong = Math.Min(
                    (chiTietMoiTonTai.SoLuong ?? 0) + data.SoLuong, tonKho);
            }
            else
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaBienThe = data.MaBienTheMoi,
                    SoLuong = Math.Min(data.SoLuong, tonKho)
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        //[Authorize]
        //public async Task<IActionResult> SelectAddress(int maDc)
        //{
        //    var user = await GetCurrentUser();
        //    if (user == null) return Json(new { success = false });

        //    await _context.Entry(user).Collection(u => u.DiaChiGiaoHangs).LoadAsync();

        //    var dc = user.DiaChiGiaoHangs.FirstOrDefault(d => d.MaDiaChi == maDc);
        //    if (dc == null) return Json(new { success = false });

        //    var newDc = new DiaChiGiaoHang
        //    {
        //        MaNguoiDung = dc.MaNguoiDung,
        //        DiaChi = dc.DiaChi,
        //        SdtNhanHang = dc.SdtNhanHang,
        //        GhiChu = dc.GhiChu
        //    };
        //    _context.DiaChiGiaoHangs.Remove(dc);
        //    _context.DiaChiGiaoHangs.Add(newDc);
        //    await _context.SaveChangesAsync();

        //    return Json(new { success = true });
        //}

    }
}