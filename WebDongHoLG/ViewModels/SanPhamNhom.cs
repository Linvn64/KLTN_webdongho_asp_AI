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

        public List<SanPhamVM> DanhSachTheoDoiTuong { get; set; } = new();

        public int MaSpDaiDien { get; set; }
    }
}
