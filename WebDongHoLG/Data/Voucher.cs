using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class Voucher
{
    public int MaVoucher { get; set; }

    public string TenVoucher { get; set; } = null!;

    public double PhanTramGiam { get; set; }

    public double GiaTriToiThieu { get; set; }
    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public int? SoLuong { get; set; }

    public int? DaDung { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
