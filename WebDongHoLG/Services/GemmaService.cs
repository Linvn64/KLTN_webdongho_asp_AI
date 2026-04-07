// GemmaService.cs
// Đặt file này vào thư mục: Services/GemmaService.cs
// Cài package: Tools → NuGet Package Manager → Console → "dotnet add package Google.GenAI"

using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebDongHoLG.Data;

namespace WebDongHoLG.Services
{
    public class GemmaService
    {
        private readonly Client _client;
        private readonly string _model = "gemma-4-31b-it";
        private readonly ShopDongHoDbContext _db;

        public GemmaService(ShopDongHoDbContext db, IConfiguration configuration)
        {
            _db = db;

            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Lỗi: Không tìm thấy 'Gemini:ApiKey' trong cấu hình appsettings.json hoặc User Secrets!");
            }

            _client = new Client(apiKey: apiKey);
        }

        // ─────────────────────────────────────────────────────────────
        // 1. Lấy dữ liệu sản phẩm từ DB → đưa vào prompt cho AI
        // ─────────────────────────────────────────────────────────────


        private async Task<string> GetFaqContextAsync()
        {
            var faqs = await _db.Faqs
                .Where(f => f.TrangThai == "Hiển thị") // Khớp với chữ 'Hiển thị' trong SQL bạn vừa chạy
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("\n=== CÂU HỎI THƯỜNG GẶP (FAQ) ===");
            foreach (var f in faqs)
            {
                sb.AppendLine($"Hỏi: {f.CauHoi}");
                sb.AppendLine($"Đáp: {f.CauTraLoi}");
            }
            return sb.ToString();
        }

        private async Task<string> GetProductContextAsync()
        {
            var sanPhams = await _db.SanPhams
        .Include(s => s.ThuongHieuNavigation)
        .Include(s => s.BienTheSanPhams)
            .ThenInclude(bt => bt.Khos)
        .Where(s => s.IsActive == true)
        .Take(20)
        .ToListAsync();

            var sb = new StringBuilder();
            foreach (var sp in sanPhams)
            {
                sb.AppendLine($"\nĐồng hồ: {sp.TenSanPham} (Thương hiệu: {sp.ThuongHieuNavigation?.TenThuongHieu})");
                sb.AppendLine($"- Link gốc: /SanPhams/Detail/{sp.MaSp}");
                sb.AppendLine("- Các phiên bản (Biến thể) hiện có:");

                foreach (var bt in sp.BienTheSanPhams.Where(b => b.IsActive))
                {
                    int tonKhoBienThe = bt.Khos.Sum(k => k.SoLuongTon ?? 0);
                    string tinhTrang = tonKhoBienThe > 0 ? $"Còn hàng ({tonKhoBienThe} chiếc)" : "Hết hàng";

                    sb.AppendLine($"  + Phiên bản: Màu {bt.MauSac}, Size {bt.DuongKinhMat}mm, Dây {bt.ChatLieuDay}");
                    sb.AppendLine($"    | Giá: {bt.GiaBan?.ToString("N0") ?? "Liên hệ"}đ");
                    sb.AppendLine($"    | Tình trạng: {tinhTrang}");
                    sb.AppendLine($"    | Ảnh riêng: {bt.ImageUrl}"); // Link ảnh riêng của từng màu/size
                }
            }
            return sb.ToString();
        }
        // ─────────────────────────────────────────────────────────────
        // 2. Gửi câu hỏi → nhận phản hồi streaming (real-time)
        // ─────────────────────────────────────────────────────────────
        public async Task StreamAsync(
            string question,
            List<ChatMessage> history,
            Func<string, Task> onChunk)
        {
            var productContext = await GetProductContextAsync();
            var faqContext = await GetFaqContextAsync();

            //var systemPrompt = $@"Bạn là chuyên gia tư vấn đồng hồ của cửa hàng WebDongHoLG.
            //                Hãy giúp khách hàng tìm được chiếc đồng hồ phù hợp nhất dựa trên nhu cầu và ngân sách.

            //                {productContext}

            //                Nguyên tắc tư vấn:
            //                - Luôn thân thiện, lịch sự, chuyên nghiệp
            //                - Chỉ tư vấn sản phẩm có trong danh sách trên
            //                - Khi gợi ý sản phẩm, nêu rõ tên và ID để khách xem chi tiết
            //                - Trả lời bằng tiếng Việt
            //                - KHÔNG hiển thị quá trình suy nghĩ, phân tích hay các bước xử lý (Thinking process).
            //                - CHỈ hiển thị câu trả lời cuối cùng cho khách hàng.
            //                - Nếu cần, dùng Google Search để tra thêm thông tin thương hiệu";



            var systemPrompt = $@"Bạn là chuyên gia tư vấn của LGWATCH.
            
            DỮ LIỆU CỬA HÀNG:
            {productContext}
            {faqContext}

            QUY TẮC PHẢN HỒI (BẮT BUỘC):
            1. ƯU TIÊN FAQ: Nếu câu hỏi nằm trong danh sách FAQ (như cách chọn size, bảo hành, chống nước), hãy trả lời theo đúng nội dung FAQ đó.
            2. TRA CỨU SẢN PHẨM: Đối chiếu chính xác Màu/Size và Tình trạng kho. Nếu hết hàng phải báo khách.
            3. GOOGLE SEARCH: Nếu khách hỏi về các kiến thức đồng hồ nằm ngoài dữ liệu trên (ví dụ: lịch sử thương hiệu Rolex, cách phân biệt thật giả, xu hướng năm 2026), hãy sử dụng công cụ Google Search để cung cấp thông tin chính xác nhất.
            4. KHÔNG HIỂN THỊ PHẦN SUY NGHĨ (THINKING), TUYỆT ĐỐI KHÔNG ĐƯỢC HIỂN THỊ SUY NGHĨ NHÉ. 
            5. TRÌNH BÀY CỰC GỌN: Dùng gạch đầu dòng (•) và in đậm (**).
            6. XUẤT CARD: Khi gợi ý sản phẩm cụ thể, luôn kèm tag: [CARD:ID|Tên sản phẩm|Giá|Ảnh]
            7. Trả lời bằng tiếng Việt thân thiện.";

            var contents = new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                },
                new Content
                {
                    Role = "model",
                    Parts = new List<Part> { new Part { Text = "Xin chào! Tôi là tư vấn viên đồng hồ WebDongHoLG. Bạn cần tư vấn gì ạ?" } }
                }
            };

            foreach (var msg in history)
            {
                contents.Add(new Content
                {
                    Role = msg.Role,
                    Parts = new List<Part> { new Part { Text = msg.Content } }
                });
            }

            contents.Add(new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = question } }
            });

            var config = new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig { ThinkingLevel = "HIGH" },
                Tools = new List<Tool> { new Tool { GoogleSearch = new GoogleSearch() } }
            };

            await foreach (var chunk in _client.Models.GenerateContentStreamAsync(_model, contents, config))
            {
                var text = chunk?.Candidates?[0]?.Content?.Parts?[0]?.Text;
                if (!string.IsNullOrEmpty(text))
                    await onChunk(text);
            }
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = "";
    }
}