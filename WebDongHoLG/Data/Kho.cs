using System;
using System.Collections.Generic;

namespace WebDongHoLG.Data;

public partial class Kho
{
    public int IdKho { get; set; }

    public int? SoLuongTon { get; set; }

    public int MaBienThe { get; set; }

    public virtual BienTheSanPham MaBienTheNavigation { get; set; } = null!;
}
