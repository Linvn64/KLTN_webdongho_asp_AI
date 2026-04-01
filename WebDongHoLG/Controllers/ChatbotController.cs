using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebDongHoLG.Services.Chatbot;

namespace WebDongHoLG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Tin nhắn không được để trống." });
            }

            try
            {
                // Gọi sang Service Groq AI của bạn. 
                // Tạm thời để maNguoiDung = null (dành cho khách chưa đăng nhập)
                var response = await _chatbotService.ChatAsync(request.Message, null);

                // Trả về biến 'reply' để khớp với code JavaScript của bạn
                return Ok(new { reply = response });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi xử lý AI", details = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}