using System;
using System.Collections.Generic;

namespace WebDongHoLG.Models;

public partial class ThongSoSanPham
{
    public int IdThongSo { get; set; }

    public int? MaSp { get; set; }

    public string? TenThongSo { get; set; }

    public string? GiaTri { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
