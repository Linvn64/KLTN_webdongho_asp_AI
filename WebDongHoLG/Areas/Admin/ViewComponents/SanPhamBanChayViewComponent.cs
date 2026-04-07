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
                    ct.MaBienTheNavigation.MaSp,
                    ct.MaBienTheNavigation.MaSpNavigation.TenSanPham
                })
                .Select(g => new {
                    TenSP = g.Key.TenSanPham,
                    AnhSP = g.Select(x => x.MaBienTheNavigation.ImageUrl)
                              .Where(url => url != null)
                              .FirstOrDefault()
                          ?? g.Select(x => x.MaBienTheNavigation.HinhAnhBienThes
                                            .Where(h => h.LaAnhChinh == true)
                                            .Select(h => h.ImageUrl)
                                            .FirstOrDefault())
                              .FirstOrDefault(),
                    SoLuongBan = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View(topSanPhams);
        }
    }
}