using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.ViewComponents
{
    public class SanPhamBanChayViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;
        public SanPhamBanChayViewComponent(ShopDongHoDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var topSanPhams = await _context.ChiTietDonHangs
                .Where(ct => ct.MaDonHangNavigation.TrangThai == "Hoàn thành")
                .GroupBy(ct => new {
                    ct.MaBienTheNavigation.MaSpNavigation.TenSanPham,
                    ct.MaBienTheNavigation.ImageUrl
                })
                .Select(g => new {
                    TenSP = g.Key.TenSanPham,
                    AnhSP = g.Key.ImageUrl,
                    SoLuongBan = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View(topSanPhams);
        }
    }
}