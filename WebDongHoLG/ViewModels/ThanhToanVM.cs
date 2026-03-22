using WebDongHoLG.Data;
using WebDongHoLG.Models;

namespace WebDongHo.ViewModels
{
    public class ThanhToanVM
    {
        public int IdThanhToan { get; set; }

        public int? MaDonHang { get; set; }

        public string? PhuongThuc { get; set; }

        public string? TrangThai { get; set; }

        public DateTime? ThoiGianThanhToan { get; set; }

        public virtual DonHang? MaDonHangNavigation { get; set; }
    }
}
