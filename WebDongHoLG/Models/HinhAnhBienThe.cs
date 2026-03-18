using System;
using System.Collections.Generic;

namespace WebDongHoLG.Models;

public partial class HinhAnhBienThe
{
    public int IdHinhAnh { get; set; }

    public int? MaBienThe { get; set; }

    public string? ImageUrl { get; set; }

    public bool? LaAnhChinh { get; set; }

    public int? ThuTuHienThi { get; set; }

    public virtual BienTheSanPham? MaBienTheNavigation { get; set; }
}
