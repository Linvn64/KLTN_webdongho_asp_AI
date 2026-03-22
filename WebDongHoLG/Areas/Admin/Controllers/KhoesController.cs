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
    public class KhoesController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        private const int NGUONG_CANH_BAO = 5;
        public KhoesController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Khoes
        public async Task<IActionResult> Index(string? searchString, int? thuongHieuId, bool? sapHetHang)
        {
            var query = _context.SanPhams
                .Include(s => s.ThuongHieuNavigation)
                .Include(s => s.BienTheSanPhams)
                    .ThenInclude(bt => bt.Khos)
                .AsQueryable();

            if (thuongHieuId.HasValue)
                query = query.Where(s => s.ThuongHieuId == thuongHieuId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(s =>
                    s.TenSanPham.ToLower().Contains(searchString) ||
                    s.BienTheSanPhams.Any(bt => bt.MaSku.ToLower().Contains(searchString)));
            }

            if (sapHetHang == true)
            {
                query = query.Where(s =>
                    s.BienTheSanPhams.Any(bt =>
                        bt.Khos.Any(k => k.SoLuongTon <= NGUONG_CANH_BAO)));
            }

            var sanPhams = await query.OrderBy(s => s.TenSanPham).ToListAsync();

            // Thống kê tổng quan
            var tatCaKho = await _context.Khos.ToListAsync();
            ViewBag.TongBienThe = tatCaKho.Count;
            ViewBag.TongTon = tatCaKho.Sum(k => k.SoLuongTon ?? 0);
            ViewBag.SapHetHang = tatCaKho.Count(k => k.SoLuongTon <= NGUONG_CANH_BAO);
            ViewBag.HetHang = tatCaKho.Count(k => k.SoLuongTon <= 0);
            ViewBag.ThuongHieus = await _context.ThuongHieus.ToListAsync();
            ViewBag.NguongCanhBao = NGUONG_CANH_BAO;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentThuongHieu = thuongHieuId;
            ViewBag.CurrentSapHetHang = sapHetHang;

            return View(sanPhams);
        }



        // GET: Admin/Khoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Khos
                .Include(k => k.MaBienTheNavigation)
                .FirstOrDefaultAsync(m => m.IdKho == id);
            if (kho == null)
            {
                return NotFound();
            }

            return View(kho);
        }

        // GET: Admin/Khoes/Create
        public IActionResult Create()
        {
            ViewData["MaBienThe"] = new SelectList(_context.BienTheSanPhams, "MaBienThe", "MaBienThe");
            return View();
        }

        // POST: Admin/Khoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKho,SoLuongTon,MaBienThe")] Kho kho)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kho);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaBienThe"] = new SelectList(_context.BienTheSanPhams, "MaBienThe", "MaBienThe", kho.MaBienThe);
            return View(kho);
        }

        // GET: Admin/Khoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Khos.FindAsync(id);
            if (kho == null)
            {
                return NotFound();
            }
            ViewData["MaBienThe"] = new SelectList(_context.BienTheSanPhams, "MaBienThe", "MaBienThe", kho.MaBienThe);
            return View(kho);
        }

        // POST: Admin/Khoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKho,SoLuongTon,MaBienThe")] Kho kho)
        {
            if (id != kho.IdKho)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoExists(kho.IdKho))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaBienThe"] = new SelectList(_context.BienTheSanPhams, "MaBienThe", "MaBienThe", kho.MaBienThe);
            return View(kho);
        }

        // GET: Admin/Khoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Khos
                .Include(k => k.MaBienTheNavigation)
                .FirstOrDefaultAsync(m => m.IdKho == id);
            if (kho == null)
            {
                return NotFound();
            }

            return View(kho);
        }

        // POST: Admin/Khoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kho = await _context.Khos.FindAsync(id);
            if (kho != null)
            {
                _context.Khos.Remove(kho);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KhoExists(int id)
        {
            return _context.Khos.Any(e => e.IdKho == id);
        }
    }
}
