using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Models;
using WebDongHoLG.ViewModels;

[Authorize]
public class DanhGiaController : Controller
{
    private readonly ShopDongHoDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DanhGiaController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int maSp, int maDonHang, int maBienThe)
    {
        var userId = _userManager.GetUserId(User);
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return RedirectToAction("Login", "Account");

        var chiTiet = await _context.ChiTietDonHangs
            .Include(ct => ct.MaBienTheNavigation).ThenInclude(bt => bt.MaSpNavigation)
            .Include(ct => ct.MaDonHangNavigation)
            .FirstOrDefaultAsync(ct => ct.MaDonHang == maDonHang &&
                                     ct.MaBienThe == maBienThe && 
                                     ct.MaBienTheNavigation.MaSp == maSp &&
                                     ct.MaDonHangNavigation.MaNguoiDung == user.MaNguoiDung &&
                                     ct.MaDonHangNavigation.TrangThai.Contains("Hoàn thành"));

        if (chiTiet == null) return RedirectToAction("Index", "DonHangKhach");

        ViewBag.MaSp = maSp;
        ViewBag.MaDonHang = maDonHang;
        ViewBag.MaBienThe = chiTiet.MaBienThe; 
        ViewBag.TenSp = chiTiet.MaBienTheNavigation.MaSpNavigation.TenSanPham;
        ViewBag.PhanLoai = $"{chiTiet.MaBienTheNavigation.MauSac} - {chiTiet.MaBienTheNavigation.DuongKinhMat}mm";
        ViewBag.HinhAnh = chiTiet.MaBienTheNavigation.ImageUrl;

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(int maSp, int maDonHang, int maBienThe, int soSao, string noiDung)
    {
        var userId = _userManager.GetUserId(User);
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return RedirectToAction("Login", "Account");

        bool daCoDanhGia = await _context.DanhGia.AnyAsync(dg =>
            dg.MaNguoiDung == user.MaNguoiDung &&
            dg.MaDonHang == maDonHang &&
            dg.MaBienThe == maBienThe);

        if (daCoDanhGia)
        {
            TempData["ToastError"] = "Bạn đã đánh giá sản phẩm này rồi!";
            return RedirectToAction("Index", "DonHangKhach");
        }

        var review = new DanhGium
        {
            MaNguoiDung = user.MaNguoiDung,
            MaSp = maSp,
            MaDonHang = maDonHang,
            MaBienThe = maBienThe,
            SoSao = soSao,
            NoiDung = noiDung?.Trim(),
            NgayDanhGia = DateTime.Now
        };

        _context.DanhGia.Add(review);
        await _context.SaveChangesAsync();

        TempData["ToastSuccess"] = "Đánh giá của bạn đã được ghi nhận!";
        return RedirectToAction("Index", "DonHangKhach", new { status = "Hoàn thành" });
    }
}