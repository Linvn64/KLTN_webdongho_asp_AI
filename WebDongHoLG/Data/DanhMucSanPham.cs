using System;
using System.Collections.Generic;
using WebDongHoLG.Data;

namespace WebDongHoLG.Models;

public partial class DanhMucSanPham
{
    public int IdDanhMuc { get; set; }

    public string? TenDanhMuc { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
