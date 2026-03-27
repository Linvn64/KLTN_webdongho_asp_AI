using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Areas.Admin.ViewModels;
using WebDongHoLG.Data;
using WebDongHoLG.ViewModels;

namespace WebDongHoLG.Areas.Admin.ViewComponents
{
    public class BieuDoDoanhThuViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;
        public BieuDoDoanhThuViewComponent(ShopDongHoDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ngayGanDay = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            var duLieu = new BieuDoDoanhThuVM
            {
                SoLieu = new List<decimal>(),
                NgayThang = new List<string>()
            };

            foreach (var ngay in ngayGanDay)
            {
                var doanhThuNgay = await _context.DonHangs
                    .Where(d => d.NgayDat.HasValue &&
                                d.NgayDat.Value.Date == ngay.Date &&
                                d.TrangThai == "Hoàn thành")
                    .SumAsync(d => d.TongTien ?? 0);

                duLieu.SoLieu.Add(doanhThuNgay);
                duLieu.NgayThang.Add(ngay.ToString("dd/MM")); 
            }

            return View(duLieu);
        }
    }
}