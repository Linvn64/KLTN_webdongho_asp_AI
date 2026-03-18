using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class LichSuDonHang
{
    public int IdLichSu { get; set; }

    public int? MaDonHang { get; set; }

    public string? TrangThaiCu { get; set; }

    public string? TrangThaiMoi { get; set; }

    public DateTime? ThoiGianCapNhat { get; set; }

    public string? NguoiCapNhat { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }
}
