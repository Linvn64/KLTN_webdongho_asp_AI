using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
