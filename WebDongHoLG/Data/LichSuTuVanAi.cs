using System;
using System.Collections.Generic;

namespace WebDongHoLG.Data;

public partial class LichSuTuVanAi
{
    public int MaTuVan { get; set; }

    public int? MaNguoiDung { get; set; }

    public string? CauHoi { get; set; }

    public string? CauTraLoi { get; set; }

    public DateTime? ThoiGian { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
