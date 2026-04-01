using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebDongHoLG.Data;
using WebDongHoLG.Models;

namespace WebDongHoLG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ShopDongHoDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var listProduct = await _context.SanPhams
                .Include(s => s.ThuongHieuNavigation) 
                .Include(s => s.BienTheSanPhams)      
                .Where(s => s.IsActive == true)
                .ToListAsync();

            return View(listProduct);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Ví dụ trong Controllers/HomeController.cs
        public async Task<IActionResult> Faq()
        {
            // Chỉ lấy các FAQ được phép hiển thị
            var danhSachFaq = await _context.Faqs
                .Where(f => f.TrangThai == "Hoạt động")
                .ToListAsync();

            return View(danhSachFaq);
        }
    }
}
