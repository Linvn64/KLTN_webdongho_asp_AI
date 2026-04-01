using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class SanPhamsController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public SanPhamsController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idDanhmuc, int? idThuongHieu, string doiTuong)
        {
            var query = _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.ThuongHieuNavigation)
                .Where(s => s.IsActive == true)
                .AsQueryable();

            if (idDanhmuc.HasValue) query = query.Where(s => s.IdDanhMuc == idDanhmuc);
            if (idThuongHieu.HasValue) query = query.Where(s => s.ThuongHieuId == idThuongHieu);
            if (!string.IsNullOrEmpty(doiTuong)) query = query.Where(s => s.DoiTuong == doiTuong);

            ViewBag.DanhMucSanPhams = _context.DanhMucSanPhams.ToList();
            ViewBag.ThuongHieus = _context.ThuongHieus.ToList();
            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["IdDanhMuc"] = new SelectList(_context.DanhMucSanPhams, "IdDanhMuc", "TenDanhMuc");
            ViewData["ThuongHieuId"] = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSp,TenSanPham,IdDanhMuc,NgayTao,MoTa,ThuongHieuId,DoiTuong,IsActive")] SanPham sanPham)
        {
            var isDuplicate = await _context.SanPhams.AnyAsync(s =>
                            s.TenSanPham == sanPham.TenSanPham &&
                            s.ThuongHieuId == sanPham.ThuongHieuId &&
                            s.IdDanhMuc == sanPham.IdDanhMuc &&
                            s.DoiTuong == sanPham.DoiTuong
    );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Sản phẩm này đã tồn tại! (Trùng tên + thương hiệu + danh mục + đối tượng)");
            }

            if (ModelState.IsValid)
            {
                sanPham.NgayTao = DateTime.Now;
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDanhMuc"] = new SelectList(_context.DanhMucSanPhams, "IdDanhMuc", "TenDanhMuc", sanPham.IdDanhMuc);
            ViewData["ThuongHieuId"] = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu", sanPham.ThuongHieuId);
            return View(sanPham);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            ViewData["IdDanhMuc"] = new SelectList(_context.DanhMucSanPhams, "IdDanhMuc", "TenDanhMuc", sanPham.IdDanhMuc);
            ViewData["ThuongHieuId"] = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu", sanPham.ThuongHieuId);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSp,TenSanPham,IdDanhMuc,NgayTao,MoTa,ThuongHieuId,DoiTuong,IsActive")] SanPham sanPham)
        {


            if (id != sanPham.MaSp) return NotFound();

            var isDuplicate = await _context.SanPhams.AnyAsync(s =>
                s.TenSanPham == sanPham.TenSanPham &&
                s.ThuongHieuId == sanPham.ThuongHieuId &&
                s.IdDanhMuc == sanPham.IdDanhMuc &&
                s.DoiTuong == sanPham.DoiTuong &&
                s.MaSp != id
            );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Sản phẩm này đã tồn tại! (Trùng tên + thương hiệu + danh mục + đối tượng)");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSp)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdDanhMuc"] = new SelectList(_context.DanhMucSanPhams, "IdDanhMuc", "TenDanhMuc", sanPham.IdDanhMuc);
            ViewData["ThuongHieuId"] = new SelectList(_context.ThuongHieus, "Id", "TenThuongHieu", sanPham.ThuongHieuId);
            return View(sanPham);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.ThuongHieuNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }


        // GET: Admin/SanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.ThuongHieuNavigation)
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: Admin/SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);

            if (sanPham != null)
            {
                sanPham.IsActive = false;
                _context.Update(sanPham);
                await _context.SaveChangesAsync();

                TempData["Type"] = "warning";
                TempData["Message"] = $"Đã chuyển sản phẩm '{sanPham.TenSanPham}' vào thùng rác thành công!";
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(int id) => _context.SanPhams.Any(e => e.MaSp == id);

        public async Task<IActionResult> Trash()
        {
            var trashList = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.BienTheSanPhams)
                .Where(s => s.IsActive == false || s.BienTheSanPhams.Any(bt => bt.IsActive == false))
                .ToListAsync();

            return View(trashList);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id, string type)
        {
            if (type == "SanPham")
            {
                var sp = await _context.SanPhams.FindAsync(id);
                if (sp != null)
                {
                    sp.IsActive = true;
                    _context.Update(sp);
                    TempData["Message"] = $"Đã khôi phục sản phẩm: {sp.TenSanPham}";
                }
            }
            else if (type == "BienThe")
            {
                var bt = await _context.BienTheSanPhams.FindAsync(id);
                if (bt != null)
                {
                    bt.IsActive = true;
                    _context.Update(bt);
                    TempData["Message"] = $"Đã khôi phục biến thể SKU: {bt.MaSku}";
                }
            }

            await _context.SaveChangesAsync();
            TempData["Type"] = "success";
            return RedirectToAction(nameof(Trash));
        }


    }

}