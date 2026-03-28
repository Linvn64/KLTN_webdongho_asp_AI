using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Data;
using WebDongHoLG.Models;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucSanPhamsController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public DanhMucSanPhamsController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DanhMucSanPhams
        public async Task<IActionResult> Index()
        {
            return View(await _context.DanhMucSanPhams.ToListAsync());
        }

        // GET: Admin/DanhMucSanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucSanPham = await _context.DanhMucSanPhams
                .FirstOrDefaultAsync(m => m.IdDanhMuc == id);
            if (danhMucSanPham == null)
            {
                return NotFound();
            }

            return View(danhMucSanPham);
        }

        // GET: Admin/DanhMucSanPhams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/DanhMucSanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDanhMuc,TenDanhMuc,MoTa")] DanhMucSanPham danhMucSanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMucSanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhMucSanPham);
        }

        // GET: Admin/DanhMucSanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucSanPham = await _context.DanhMucSanPhams.FindAsync(id);
            if (danhMucSanPham == null)
            {
                return NotFound();
            }
            return View(danhMucSanPham);
        }

        // POST: Admin/DanhMucSanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDanhMuc,TenDanhMuc,MoTa")] DanhMucSanPham danhMucSanPham)
        {
            if (id != danhMucSanPham.IdDanhMuc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMucSanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucSanPhamExists(danhMucSanPham.IdDanhMuc))
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
            return View(danhMucSanPham);
        }

        // GET: Admin/DanhMucSanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMucSanPham = await _context.DanhMucSanPhams
                .FirstOrDefaultAsync(m => m.IdDanhMuc == id);
            if (danhMucSanPham == null)
            {
                return NotFound();
            }

            return View(danhMucSanPham);
        }

        // POST: Admin/DanhMucSanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMucSanPham = await _context.DanhMucSanPhams.FindAsync(id);
            if (danhMucSanPham != null)
            {
                _context.DanhMucSanPhams.Remove(danhMucSanPham);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucSanPhamExists(int id)
        {
            return _context.DanhMucSanPhams.Any(e => e.IdDanhMuc == id);
        }
    }
}
