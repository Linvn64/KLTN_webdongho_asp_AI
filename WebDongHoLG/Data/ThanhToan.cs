using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class ThanhToan
{
    public int IdThanhToan { get; set; }

    public int? MaDonHang { get; set; }

    public string? PhuongThuc { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? ThoiGianThanhToan { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }
}
