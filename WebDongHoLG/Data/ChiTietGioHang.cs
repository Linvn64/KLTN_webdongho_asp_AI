using System;
using System.Collections.Generic;
using WebDongHoLG.Data;

namespace WebDongHoLG.Models;

public partial class ChiTietGioHang
{
    public int MaGioHang { get; set; }

    public int MaBienThe { get; set; }

    public int? SoLuong { get; set; }

    public virtual BienTheSanPham MaBienTheNavigation { get; set; } = null!;

    public virtual GioHang MaGioHangNavigation { get; set; } = null!;
}
