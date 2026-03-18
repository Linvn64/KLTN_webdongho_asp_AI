using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string? TenSanPham { get; set; }

    public int? IdDanhMuc { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? MoTa { get; set; }

    public string? ThuongHieu { get; set; }

    public string? DoiTuong { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual DanhMucSanPham? IdDanhMucNavigation { get; set; }

    public virtual ICollection<ThongSoSanPham> ThongSoSanPhams { get; set; } = new List<ThongSoSanPham>();
}
