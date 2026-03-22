using Microsoft.AspNetCore.Mvc;
using WebDongHoLG.Data;
using WebDongHoLG.ViewModels;

namespace WebDongHoLG.ViewComponents
{
    public class DanhMucViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;
        public DanhMucViewComponent(ShopDongHoDbContext context) => _context = context;

        public IViewComponentResult Invoke()
        {
            var data = _context.DanhMucSanPhams.Select(dm => new DanhMucVM
            {
                MaDanhMuc = dm.IdDanhMuc,
                TenDanhMuc = dm.TenDanhMuc,
                SoLuong = dm.SanPhams.Count(s => s.IsActive == true)
            }).OrderBy(p => p.TenDanhMuc);
            return View(data);
        }
    }
}