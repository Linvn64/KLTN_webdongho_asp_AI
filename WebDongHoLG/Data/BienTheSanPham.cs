using System;
using System.Collections.Generic;
using WebDongHoLG.Models;

namespace WebDongHoLG.Data;

public partial class BienTheSanPham
{
    public int MaBienThe { get; set; }

    public int MaSp { get; set; }

    public string? MauSac { get; set; }

    public int? DuongKinhMat { get; set; }

    public string? ChatLieuDay { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaNhap { get; set; }

    public string? MaSku { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<HinhAnhBienThe> HinhAnhBienThes { get; set; } = new List<HinhAnhBienThe>();

    public virtual ICollection<Kho> Khos { get; set; } = new List<Kho>();

    public virtual SanPham MaSpNavigation { get; set; } = null!;

    public virtual ICollection<NhapKho> NhapKhos { get; set; } = new List<NhapKho>();
}
