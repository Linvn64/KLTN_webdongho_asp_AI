namespace WebDongHoLG.ViewModels
{
    public class SanPhamNhomVM
    {
        public string TenSp { get; set; }
        public string? Hinh { get; set; }
        public string? MoTa { get; set; }
        public decimal? GiaBanThapNhat { get; set; }
        public string? TenDanhMuc { get; set; }
        public string? TenThuongHieu { get; set; }
        public bool ConHang { get; set; }
        public int TongLuotBan { get; set; }
        public DateTime? NgayTao { get; set; }
        public List<SanPhamVM> DanhSachTheoDoiTuong { get; set; } = new();

        public int MaSpDaiDien { get; set; }

        public bool LaBanChay => TongLuotBan > 5;
        public bool LaMoi => NgayTao.HasValue
                             && NgayTao.Value >= DateTime.Now.AddMonths(-2);
    }
}
