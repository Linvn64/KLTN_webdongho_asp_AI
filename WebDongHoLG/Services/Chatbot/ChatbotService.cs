using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using WebDongHoLG.Data;

namespace WebDongHoLG.Services.Chatbot
{
    public class ChatbotService : IChatbotService
    {
        private readonly ShopDongHoDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private const string GROQ_URL = "https://api.groq.com/openai/v1/chat/completions";

        public ChatbotService(ShopDongHoDbContext context,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration config)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _cache = cache;
            _config = config;
        }

        public async Task<string> ChatAsync(string userMessage, int? maNguoiDung)
        {
            var dbHistory = await _context.LichSuTuVanAis
            .Where(h => h.MaNguoiDung == maNguoiDung)
            .OrderByDescending(h => h.ThoiGian)
            .Take(5)
            .ToListAsync();

            var productContext = await _cache.GetOrCreateAsync("product_context", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await BuildProductContextAsync();
            });

            var systemPrompt = $@"Bạn là trợ lý tư vấn đồng hồ của cửa hàng LGWATCH.
                                Trả lời ngắn gọn, thân thiện bằng tiếng Việt.

                                QUY TẮC TUYỆT ĐỐI KHI VIẾT LINK:
                                - CHỈ dùng đúng cú pháp: [Xem sản phẩm](/SanPhams/Detail/ID)
                                - KHÔNG dùng HTML, KHÔNG dùng thẻ <a>, KHÔNG dùng style
                                - KHÔNG dùng markdown khác ngoài cú pháp trên
                                - Ví dụ ĐÚNG: Rolex Datejust giá 19,000,000đ [Xem sản phẩm](/SanPhams/Detail/5)
                                - Ví dụ SAI: <a href=...>, style=""..., display:flex...

                                Danh sách sản phẩm:{productContext}";

            // Build messages
            var messages = new List<object>
                        {
                        new { role = "system", content = systemPrompt }
                        };

            foreach (var h in dbHistory.OrderBy(h => h.ThoiGian))
            {
                messages.Add(new { role = "user", content = h.CauHoi ?? "" });
                messages.Add(new { role = "assistant", content = h.CauTraLoi ?? "" });
            }
            messages.Add(new { role = "user", content = userMessage });

            var payload = new
            {
                model = "llama-3.3-70b-versatile",
                messages,
                temperature = 0.3,
                max_tokens = 1024
            };

            var apiKey = _config["Groq:ApiKey"];
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(GROQ_URL, content);
            var result = await response.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<GroqResponse>(result);
            var reply = parsed?.choices?[0]?.message?.content;

            if (string.IsNullOrWhiteSpace(reply))
            {
                Console.WriteLine("Groq error: " + result);
                return "Xin lỗi, tôi không thể trả lời lúc này. Vui lòng thử lại!";
            }

            // Lưu vào DB
            _context.LichSuTuVanAis.Add(new LichSuTuVanAi
            {
                MaNguoiDung = maNguoiDung,
                CauHoi = userMessage,
                CauTraLoi = reply,
                ThoiGian = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return reply;
        }

        public async Task<List<LichSuTuVanAi>> GetHistoryAsync(int? maNguoiDung, int take = 50)
        {
            return await _context.LichSuTuVanAis
            .Where(h => h.MaNguoiDung == maNguoiDung)
            .OrderBy(h => h.ThoiGian)
            .Take(take)
            .ToListAsync();
        }

        private async Task<string> BuildProductContextAsync()
        {
            var sanPhams = await _context.SanPhams
            .Include(s => s.BienTheSanPhams)
            .Include(s => s.ThuongHieuNavigation)
            .Include(s => s.IdDanhMucNavigation)
            .Where(s => s.IsActive == true)
            .Take(20)
            .ToListAsync();

            var sb = new StringBuilder();
            foreach (var sp in sanPhams)
            {
                var url = $"/SanPhams/Detail/{sp.MaSp}";
                var giaMin = sp.BienTheSanPhams
                .Where(b => b.IsActive && b.GiaBan.HasValue)
                .Select(b => b.GiaBan)
                .DefaultIfEmpty(0)
                .Min();

                var anhDaiDien = sp.BienTheSanPhams
                .Where(b => b.IsActive && !string.IsNullOrEmpty(b.ImageUrl))
                .Select(b => b.ImageUrl)
                .FirstOrDefault() ?? "";
                sb.AppendLine($"-{sp.TenSanPham}" +

                $" | Thương hiệu:{sp.ThuongHieuNavigation?.TenThuongHieu}" +
                $" | Danh mục:{sp.IdDanhMucNavigation?.TenDanhMuc}" +
                $" | Đối tượng:{sp.DoiTuong}" +
                $" | Giá từ:{giaMin:N0}đ" +
                $" | Link:{url}" +
                $" | Ảnh:{anhDaiDien}"); ;

                foreach (var bt in sp.BienTheSanPhams.Where(b => b.IsActive))
                {
                    sb.AppendLine($"  + Màu:{bt.MauSac}" +
                    $" | Size:{bt.DuongKinhMat}mm" +
                    $" | Dây:{bt.ChatLieuDay}" +
                    $" | Giá:{(bt.GiaBan.HasValue ? bt.GiaBan.Value.ToString("N0") + "đ" : "Liên hệ")}");
                }
            }
            return sb.ToString();
        }

        private class GroqResponse
        {
            public List<Choice>? choices { get; set; }
            public class Choice
            {
                public Message? message { get; set; }
            }
            public class Message
            {
                public string? content { get; set; }
            }
        }
    }
}