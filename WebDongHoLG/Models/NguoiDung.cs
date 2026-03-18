using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WebDongHoLG.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string? GioiTinh { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? UserId { get; set; }

    public string? AnhDaiDien { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DiaChiGiaoHang> DiaChiGiaoHangs { get; set; } = new List<DiaChiGiaoHang>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual ICollection<LichSuTuVanAi> LichSuTuVanAis { get; set; } = new List<LichSuTuVanAi>();
    public virtual IdentityUser? IdentityUser { get; set; } 
}
