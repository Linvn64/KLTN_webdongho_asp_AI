// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WebDongHoLG.Data;

namespace WebDongHoLG.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ShopDongHoDbContext _context;

        public EmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender, 
            ShopDongHoDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }
        public bool IsLinkedGoogle { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var logins = await _userManager.GetLoginsAsync(user);
            IsLinkedGoogle = logins.Any(l => l.LoginProvider == "Google");

            await LoadAsync(user);
            return Page();
        }
        

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();
            var logins = await _userManager.GetLoginsAsync(user);
            if (logins.Any(l => l.LoginProvider == "Google"))
            {
                StatusMessage = "Error: Tài khoản đang liên kết Google, vui lòng ngắt liên kết trước khi đổi email.";
                return RedirectToPage();
            }
            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);

                var setEmailResult = await _userManager.SetEmailAsync(user, Input.NewEmail);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return Page();
                }

                var nguoiDung = _context.NguoiDungs.FirstOrDefault(n => n.UserId == userId);
                if (nguoiDung != null)
                {
                    nguoiDung.Email = Input.NewEmail;
                    await _context.SaveChangesAsync();
                }

                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Email đã được cập nhật thành công.";
                return RedirectToPage();
            }

            StatusMessage = "Email không thay đổi.";
            return RedirectToPage();
        }

    }
}
