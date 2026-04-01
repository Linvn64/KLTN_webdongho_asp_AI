using WebDongHoLG.Data;

namespace WebDongHoLG.Services.Chatbot
{
    public interface IChatbotService
    {
        Task<string> ChatAsync(string userMessage, int? maNguoiDung);
        Task<List<LichSuTuVanAi>> GetHistoryAsync(int? maNguoiDung, int take = 50);
    }
}