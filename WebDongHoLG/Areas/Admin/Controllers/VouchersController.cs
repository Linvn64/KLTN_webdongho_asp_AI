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
    public class VouchersController : Controller
    {
        private readonly ShopDongHoDbContext _context;

        public VouchersController(ShopDongHoDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Vouchers
        public async Task<IActionResult> Index()
        {
            var list = await _context.Vouchers
                .OrderByDescending(v => v.IsActive)
                .ThenByDescending(v => v.NgayBatDau)
                .ToListAsync();
            return View(list);
        }

        // GET: Admin/Vouchers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // GET: Admin/Vouchers/Create
        public IActionResult Create()
        {
            var model = new Voucher
            {
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(7),
                GiaTriToiThieu = 0,
                SoLuong = 100
            };
            return View(model);
        }

        // POST: Admin/Vouchers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaVoucher,TenVoucher,PhanTramGiam,GiaTriToiThieu,NgayBatDau,NgayKetThuc,SoLuong,IsActive")] Voucher voucher)
        {
            bool isDuplicate = await _context.Vouchers.AnyAsync(v =>
                                                                v.TenVoucher.ToUpper() == voucher.TenVoucher.ToUpper() &&
                                                                v.NgayBatDau == voucher.NgayBatDau &&
                                                                v.NgayKetThuc == voucher.NgayKetThuc);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Voucher với mã và thời gian này đã tồn tại!");
            }

            if (voucher.NgayKetThuc <= voucher.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải lớn hơn ngày bắt đầu!");
            }

            if (ModelState.IsValid)
            {
                voucher.DaDung = 0;
                voucher.TenVoucher = voucher.TenVoucher.ToUpper().Trim();
                voucher.PhanTramGiam = voucher.PhanTramGiam / 100.0;
                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Vouchers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            voucher.PhanTramGiam = voucher.PhanTramGiam * 100;
            return View(voucher);
        }

        // POST: Admin/Vouchers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaVoucher,TenVoucher,PhanTramGiam,GiaTriToiThieu,NgayBatDau,NgayKetThuc,IsActive")] Voucher voucher, int SoLuongTangThem, int SoLuongCu)
        {
            if (id != voucher.MaVoucher) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var currentData = await _context.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.MaVoucher == id);
                    voucher.DaDung = currentData.DaDung;

                    voucher.SoLuong = SoLuongCu + SoLuongTangThem;

                        voucher.TenVoucher = voucher.TenVoucher.ToUpper().Trim();
                    voucher.PhanTramGiam = voucher.PhanTramGiam / 100.0;

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.MaVoucher)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }
        // GET: Admin/Vouchers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.MaVoucher == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // POST: Admin/Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.MaVoucher == id);
        }

        public async Task<IActionResult> ToggleStatus(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            voucher.IsActive = !(voucher.IsActive ?? false);

            _context.Update(voucher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
