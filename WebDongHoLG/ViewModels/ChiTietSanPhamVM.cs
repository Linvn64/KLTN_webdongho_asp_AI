using WebDongHoLG.Models;

namespace WebDongHoLG.ViewModels
{
    public class ChiTietSanPhamVM
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; }
        public string? MoTa { get; set; }
        public string? TenDanhMuc { get; set; }
        public string? TenThuongHieu { get; set; }
        public string? DoiTuong { get; set; }
        public double SoSaoTrungBinh { get; set; }
        public int SoDanhGia { get; set; }
        public List<DanhGiaVM> DanhGias { get; set; } = new();
        public List<BienTheVM> BienThes { get; set; } = new();

        public List<DoiTuongVM> CacDoiTuong { get; set; } = new();
    }

    public class DoiTuongVM
    {
        public int MaSp { get; set; }
        public string? DoiTuong { get; set; }
        public bool DangChon { get; set; }
    }

    public class DanhGiaVM
    {
        public string? TenNguoiDung { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; }
    }

}
