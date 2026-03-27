using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using WebDongHoLG.Areas.Admin.ViewModels;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.ViewComponents
{
    public class TheThongKeViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;

        public TheThongKeViewComponent(ShopDongHoDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new TheThongKeVM
            {
                TongDoanhThu = await _context.DonHangs
                    .Where(d => d.TrangThai == "Hoàn thành")
                    .SumAsync(d => d.TongTien ?? 0),

                TongDonHang = await _context.DonHangs.CountAsync(),

                TongSanPham = await _context.SanPhams.CountAsync(),

                TongKhachHang = await _context.NguoiDungs.CountAsync()
            };

            return View(model);
        }
    }
}