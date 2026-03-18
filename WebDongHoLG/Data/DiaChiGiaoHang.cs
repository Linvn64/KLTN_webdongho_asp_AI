using System;
using System.Collections.Generic;
using WebDongHoLG.Data;

namespace WebDongHoLG.Models;

public partial class DiaChiGiaoHang
{
    public int MaDiaChi { get; set; }

    public int? MaNguoiDung { get; set; }

    public string? DiaChi { get; set; }

    public string? SdtNhanHang { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
