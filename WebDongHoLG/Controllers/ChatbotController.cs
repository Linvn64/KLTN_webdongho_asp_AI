// ChatController.cs
// Đặt file này vào thư mục: Controllers/ChatController.cs

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebDongHoLG.Services;

namespace WebDongHoLG.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")] // → /api/Chat


    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly GemmaService _gemmaService;

        public ChatController(GemmaService gemmaService)
        {
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

            var history = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(historyJson))
            {
                try
                {
                    history = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? history;
                }
                catch { /* bỏ qua nếu parse lỗi */ }
            }

            await _gemmaService.StreamAsync(question, history, async chunk =>
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

            var sb = new System.Text.StringBuilder();
            await _gemmaService.StreamAsync(req.Question, req.History ?? new List<ChatMessage>(), async chunk =>
            {
                sb.Append(chunk);
                await Task.CompletedTask;
            });

            return Ok(new { answer = sb.ToString() });
        }
    }

    public class SendRequest
    {
        public string Question { get; set; } = "";
        public List<ChatMessage>? History { get; set; }
    }
}