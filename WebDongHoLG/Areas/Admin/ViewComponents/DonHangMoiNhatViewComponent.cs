using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.ViewComponents
{
    public class DonHangMoiNhatViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;
        public DonHangMoiNhatViewComponent(ShopDongHoDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var danhSachDonHang = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .ToListAsync();

            return View(danhSachDonHang);
        }
    }
}