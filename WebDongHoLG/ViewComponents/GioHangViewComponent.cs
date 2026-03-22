using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDongHoLG.Data;

namespace WebDongHo.ViewComponents
{
    public class GioHangViewComponent : ViewComponent
    {
        private readonly ShopDongHoDbContext _context;
        public GioHangViewComponent(ShopDongHoDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var email = UserClaimsPrincipal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Content("0");

            var count = await _context.ChiTietGioHangs
                .Where(c => c.MaGioHangNavigation.MaNguoiDungNavigation.Email == email)
                .SumAsync(c => c.SoLuong) ?? 0;

            return Content(count.ToString());
        }
    }
}