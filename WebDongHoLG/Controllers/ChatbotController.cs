// ChatController.cs
// Đặt file này vào thư mục: Controllers/ChatController.cs

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebDongHoLG.Data;
using WebDongHoLG.Services;
using Microsoft.EntityFrameworkCore;

namespace WebDongHoLG.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")] // → /api/Chat


    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly GemmaService _gemmaService;
        private readonly ShopDongHoDbContext _context;  // thêm dòng này


        public ChatController(GemmaService gemmaService, ShopDongHoDbContext context)
        {
            _context = context;  // thêm dòng này

            _gemmaService = gemmaService;
        }

        [HttpGet("/Chat")]
        public IActionResult Index()
        {
            return View(); // Nó sẽ tìm file Index.cshtml trong Views/Chat/
        }



        // ─────────────────────────────────────────────────────────────
        // GET /api/Chat/stream?question=...&historyJson=...
        // Server-Sent Events — frontend nhận text từng chút real-time
        // ─────────────────────────────────────────────────────────────
        [HttpGet("stream")]
        public async Task Stream([FromQuery] string question, [FromQuery] string? historyJson = null)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            var userName = isLoggedIn ? User.Identity!.Name : null;

            var history = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(historyJson))
            {
                try
                {
                    history = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? history;
                }
                catch { }
            }

            await _gemmaService.StreamAsync(question, history, isLoggedIn, userName, async chunk =>
            {
                var json = JsonSerializer.Serialize(new { text = chunk });
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();
            });

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        // ─────────────────────────────────────────────────────────────
        // POST /api/Chat/send  (không stream — dùng khi cần đơn giản)
        // Body JSON: { "question": "...", "history": [...] }
        // ─────────────────────────────────────────────────────────────
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return BadRequest(new { error = "Câu hỏi không được để trống." });

            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            var userName = isLoggedIn ? User.Identity!.Name : null;

            // --- BẮT ĐẦU THÊM ĐOẠN NÀY ---
            string aiContext = "";
            if (isLoggedIn && !string.IsNullOrEmpty(userName))
            {
                var hasVoucher = await _context.Vouchers.AnyAsync(v => v.TenVoucher.StartsWith($"DEAL_{userName}"));
                if (hasVoucher)
                {
                    aiContext = "[HƯỚNG DẪN ẨN: Khách này ĐÃ TỪNG nhận mã giảm giá rồi. Tuyệt đối KHÔNG cấp thêm mã mới, hãy từ chối khéo léo và tập trung tư vấn sản phẩm.]\n\n";
                }
                else
                {
                    aiContext = "[HƯỚNG DẪN ẨN: Khách này CHƯA có mã giảm giá. Nếu khách xin giảm giá, hãy hỏi lý do (ví dụ: mua lần đầu, mua tặng ai) trước. Khách trả lời hợp lý thì mới cấp mã.]\n\n";
                }
            }
            string finalQuestion = $"{aiContext}Khách hỏi: {req.Question}";
            // --- KẾT THÚC THÊM ---

            var sb = new System.Text.StringBuilder();

            // CHÚ Ý: Truyền finalQuestion vào thay vì req.Question
            await _gemmaService.StreamAsync(
                finalQuestion,
                req.History ?? new List<ChatMessage>(),
                isLoggedIn,
                userName,
                async chunk =>
                {
                    sb.Append(chunk);
                    await Task.CompletedTask;
                });

            return Ok(new { answer = sb.ToString() });
        }


        [HttpPost("request-discount")]
        public async Task<IActionResult> RequestDiscount([FromBody] DiscountRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MaNguoiDung))
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập để nhận mã giảm giá." });

            var hasUsedOrHasVoucher = await _context.Vouchers
                .AnyAsync(v => v.TenVoucher.StartsWith($"DEAL_{req.MaNguoiDung}"));

            if (hasUsedOrHasVoucher)
            {
                return Ok(new
                {
                    success = false,
                    message = "Hệ thống ghi nhận bạn đã từng nhận ưu đãi đặc biệt này rồi. Mỗi khách hàng chỉ được tham gia một lần duy nhất ạ!"
                });
            }

            var voucher = new Voucher
            {
                TenVoucher = $"DEAL_{req.MaNguoiDung}",
                PhanTramGiam = (double)Math.Min(req.PhanTramGiam ?? 5, 10) / 100.0,
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(3),
                SoLuong = 1,
                DaDung = 0,
                IsActive = true,
                GiaTriToiThieu = 500000
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                maVoucher = voucher.MaVoucher,
                phanTramGiam = voucher.PhanTramGiam,
                message = $"Tạo mã giảm giá **{voucher.MaVoucher}** giảm {voucher.PhanTramGiam}% thành công! Hết hạn {voucher.NgayKetThuc:dd/MM/yyyy}."
            });
        }

        public class DiscountRequest
        {
            public string MaNguoiDung { get; set; } = "";
            public int? PhanTramGiam { get; set; }
        }
    }

    public class SendRequest
    {
        public string Question { get; set; } = "";
        public List<ChatMessage>? History { get; set; }
    }
}