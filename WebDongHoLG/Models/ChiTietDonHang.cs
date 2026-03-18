using System;
using System.Collections.Generic;

namespace WebDongHoLG.Models;

public partial class ChiTietDonHang
{
    public int MaDonHang { get; set; }

    public int MaBienThe { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGiaTaiThoiDiem { get; set; }

    public virtual BienTheSanPham MaBienTheNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;
}
