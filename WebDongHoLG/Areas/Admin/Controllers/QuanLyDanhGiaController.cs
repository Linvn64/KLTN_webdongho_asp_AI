using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyDanhGiaController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public QuanLyDanhGiaController(ShopDongHoDbContext context) => _context = context; 

        public async Task<IActionResult> Index(int? maSp, int? soSao)
        {
            var query = _context.DanhGia
                .Include(dg => dg.MaNguoiDungNavigation)
                .Include(dg => dg.MaSpNavigation)
                .AsQueryable(); 

            if (maSp.HasValue)
            {
                query = query.Where(dg => dg.MaSp == maSp); 
            }

            if (soSao.HasValue)
            {
                query = query.Where(dg => dg.SoSao == soSao); 
            }

            var danhGias = await query
                .OrderByDescending(dg => dg.NgayDanhGia)
                .ToListAsync();

            ViewBag.DanhSachSanPham = await _context.SanPhams
                 .OrderBy(s => s.TenSanPham)
                 .Select(s => new { s.MaSp, s.TenSanPham })
                 .ToListAsync();

            ViewBag.CurrentMaSp = maSp;
            ViewBag.CurrentSoSao = soSao;

            return View(danhGias);
        }


        // GET: Admin/QuanLyDanhGia/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dg = await _context.DanhGia
                .Include(d => d.MaSpNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (dg == null) return NotFound();

            return View(dg); 
        }

        // POST: Admin/QuanLyDanhGia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dg = await _context.DanhGia.FindAsync(id);
            if (dg != null)
            {
                _context.DanhGia.Remove(dg);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
