using System;
using System.Collections.Generic;

namespace WebDongHoLG.Data;

public partial class NhapKho
{
    public int Id { get; set; }

    public int SoLuongNhap { get; set; }

    public DateTime NgayNhap { get; set; }

    public string? NguoiNhap { get; set; }

    public decimal GiaNhapLuuTru { get; set; }

    public int? MaBienThe { get; set; }

    public virtual BienTheSanPham? MaBienTheNavigation { get; set; }
}
