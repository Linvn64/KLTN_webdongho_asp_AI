using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Areas.Admin.ViewModels;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BaoCaoController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public BaoCaoController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> TaiChinh(DateTime? tuNgay, DateTime? denNgay)
        {
            DateTime vTuNgay = tuNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime vDenNgay = denNgay ?? DateTime.Now;

            var donHangs = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaBienTheNavigation)
                .Where(d => d.NgayDat >= vTuNgay && d.NgayDat <= vDenNgay.AddDays(1).AddTicks(-1) && d.TrangThai == "Hoàn thành")
                .ToListAsync();

            var tatCaBienThe = await _context.BienTheSanPhams.Include(x => x.Khos).ToListAsync();

            var model = new BaoCaoTaiChinhVM
            {
                TuNgay = vTuNgay,
                DenNgay = vDenNgay,
                TongSoLuongTon = tatCaBienThe.Sum(x => x.Khos.Sum(k => k.SoLuongTon ?? 0)),
                TongVonTonKho = tatCaBienThe.Sum(x => (x.Khos.Sum(k => k.SoLuongTon ?? 0)) * (x.GiaNhap ?? 0m)),
                TienVao = donHangs.Sum(d => d.TongTien - d.PhiVanChuyen) ?? 0m,

                TienRa = donHangs.SelectMany(d => d.ChiTietDonHangs)
                     .Sum(ct => (ct.SoLuong ?? 0) * (ct.MaBienTheNavigation?.GiaNhap ?? 0m)),

                GiaTriBanRaDuKien = (decimal)tatCaBienThe.Sum(x => (x.Khos.Sum(k => k.SoLuongTon ?? 0)) * (x.GiaBan ?? 0)),
                SanPhamSapHetHang = tatCaBienThe.Count(x => x.Khos.Sum(k => k.SoLuongTon ?? 0) <= 5)
            };
            model.LoiNhuanGop = model.TienVao - model.TienRa;

            return View(model);
        }
    }
}