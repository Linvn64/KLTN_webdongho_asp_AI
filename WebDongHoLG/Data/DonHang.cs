using System;
using System.Collections.Generic;
using WebDongHoLG.Data;

namespace WebDongHoLG.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? MaDiaChi { get; set; }

    public DateTime? NgayDat { get; set; }

    public decimal? TongTien { get; set; }

    public string? TrangThai { get; set; }

    public int? MaVoucher { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<LichSuDonHang> LichSuDonHangs { get; set; } = new List<LichSuDonHang>();

    public virtual DiaChiGiaoHang? MaDiaChiNavigation { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Voucher? MaVoucherNavigation { get; set; }

    public virtual ThanhToan? ThanhToan { get; set; }
}
