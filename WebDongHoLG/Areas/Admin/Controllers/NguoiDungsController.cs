using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDongHoLG.Areas.Admin.Models;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NguoiDungsController : Controller
    {
        private readonly ShopDongHoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public NguoiDungsController(ShopDongHoDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, string roleFilter, DateTime? dateFilter)
        {
            ViewData["CurrentFilter"] = searchString;

            var query = from nd in _context.NguoiDungs
                        join userRole in _context.UserRoles on nd.UserId equals userRole.UserId into roles
                        from r in roles.DefaultIfEmpty()
                        join role in _context.Roles on r.RoleId equals role.Id into roleNames
                        from rn in roleNames.DefaultIfEmpty()
                        select new NguoiDungViewModel
                        {
                            MaNguoiDung = nd.MaNguoiDung,
                            HoTen = nd.HoTen,
                            Email = nd.Email,
                            Sdt = nd.Sdt,
                            NgayTao = nd.NgayTao,
                            UserId = nd.UserId,
                            AnhDaiDien = nd.AnhDaiDien,
                            GioiTinh = nd.GioiTinh,
                            VaiTro = rn.Name ?? "Khách hàng"
                        };

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.HoTen.Contains(searchString) || s.Email.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(s => s.VaiTro.Equals(roleFilter));
            }
            if (dateFilter.HasValue)
            {
                query = query.Where(s => s.NgayTao.HasValue && s.NgayTao.Value.Date == dateFilter.Value.Date);
            }

            var result = await query.OrderByDescending(n => n.NgayTao).ToListAsync();
            return View(result);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await (from nd in _context.NguoiDungs
                              where nd.MaNguoiDung == id 
                              join ur in _context.UserRoles on nd.UserId equals ur.UserId into userRoles
                              from ur in userRoles.DefaultIfEmpty()
                              join r in _context.Roles on ur.RoleId equals r.Id into roles
                              from r in roles.DefaultIfEmpty()
                              select new NguoiDungViewModel
                              {
                                  MaNguoiDung = nd.MaNguoiDung,
                                  HoTen = nd.HoTen,
                                  GioiTinh = nd.GioiTinh,
                                  Email = nd.Email,
                                  Sdt = nd.Sdt,
                                  AnhDaiDien = nd.AnhDaiDien,
                                  NgayTao = nd.NgayTao,
                                  UserId = nd.UserId, 
                                  VaiTro = r.Name ?? "Khách hàng"
                              }).FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return View(user);
        }

        // GET: Admin/NguoiDungs/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.UserId == user.Id);
            if (profile == null) return NotFound();

            var model = new EditProfileViewModel
            {
                HoTen = profile.HoTen,
                Sdt = profile.Sdt,
                GioiTinh = profile.GioiTinh,
                Email = profile.Email,
                AnhDaiDien = profile.AnhDaiDien
            };

            return View(model);
        }

        // POST: Admin/NguoiDungs/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.UserId == user.Id);

            if (profile == null) return NotFound();

            if (model.UploadAnh != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.UploadAnh.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatarUploads", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.UploadAnh.CopyToAsync(stream);
                }

                profile.AnhDaiDien = fileName;
            }

            profile.HoTen = model.HoTen;
            profile.Sdt = model.Sdt;
            profile.GioiTinh = model.GioiTinh;

            _context.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = "Đã cập nhật quyền hạn cho " + user.Email;
            return RedirectToAction("Index");
        }

        [HttpGet]

        public async Task<IActionResult> PhanQuyen(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.MaNguoiDung == id);
            if (nguoiDung == null) return NotFound();

            var user = await _userManager.FindByIdAsync(nguoiDung.UserId);

            var existingClaims = await _userManager.GetClaimsAsync(user);

            var model = new PhanQuyenViewModel
            {
                UserId = user.Id,
                HoTen = nguoiDung.HoTen,
                QuyenHienTai = existingClaims.Select(c => c.Value).ToList(),
                TatCaChucNang = new List<string> { "SanPham", "DonHang", "Kho", "AI", "Reviews" }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LuuPhanQuyen(string userId, List<string> selectedClaims)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var claims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, claims);

            if (selectedClaims != null)
            {
                foreach (var claimValue in selectedClaims)
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("MenuAccess", claimValue));
                }
            }

            TempData["Success"] = "Đã phân quyền thành công!";
            return RedirectToAction("Index");
        }
    }
}
