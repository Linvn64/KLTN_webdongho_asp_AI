using WebDongHoLG.Data;
using WebDongHoLG.Models;

namespace WebDongHoLG.ViewModels
{
    public class SanPhamVM
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; }
        public string? Hinh { get; set; }      
        public string? MoTa { get; set; }
        public decimal? GiaBan { get; set; }  
        public decimal? GiaCao { get; set; }   
        public string? TenDanhMuc { get; set; }
        public string? TenThuongHieu { get; set; }
        public string? DoiTuong { get; set; }
        public int SoBienThe { get; set; }
        public bool ConHang { get; set; }
        public int LuotBan { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    //public class ChiTietSanPhamVM
    //{
    //    public int MaSp { get; set; }
    //    public string TenSp { get; set; }
    //    public string? MoTa { get; set; }
    //    public string? TenDanhMuc { get; set; }
    //    public string? TenThuongHieu { get; set; }
    //    public string? DoiTuong { get; set; }

    //    public List<BienTheVM> BienThes { get; set; } = new();

    //    public double SoSaoTrungBinh { get; set; }
    //    public int SoDanhGia { get; set; }
    //    public List<DanhGiaVM> DanhGias { get; set; } = new();
    //}

    public class BienTheVM
    {
        public int MaBienThe { get; set; }
        public string? MaSku { get; set; }
        public string? MauSac { get; set; }
        public int? DuongKinhMat { get; set; }
        public string? ChatLieuDay { get; set; }
        public decimal? GiaBan { get; set; }
        public decimal? GiaNhap { get; set; }
        public string? ImageUrl { get; set; }
        public int SoLuongTon { get; set; }
        public List<string> HinhAnhs { get; set; } = new();
        
    }

    //public class DanhGiaVM
    //{
    //    public string? TenNguoiDung { get; set; }
    //    public int SoSao { get; set; }
    //    public string? NoiDung { get; set; }
    //    public DateTime NgayDanhGia { get; set; }
    //}

    public class DanhMucVM
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public int SoLuong { get; set; }
    }


    // Thêm vào ViewModels/SanPhamVM.cs
    public class GioHangVM
    {
        public int MaBienThe { get; set; }
        public string TenSp { get; set; }
        public string? TenThuongHieu { get; set; }
        public string? MauSac { get; set; }
        public int? DuongKinhMat { get; set; }
        public string? ChatLieuDay { get; set; }
        public string? MaSku { get; set; }
        public string? Hinh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public int SoLuongTon { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
        public int MaSpGoc { get; set; }

    }

    public class GioHangItemVM
    {
        public int MaBienThe { get; set; }
        public int SoLuong { get; set; }
        public int Id { get; set; }
        public int Quantity { get; set; }

    }

    public class DonHangVM
    {
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SdtNhanHang { get; set; }
        public string? TinhThanh { get; set; }
        public string? DiaChiChiTiet { get; set; }
        public string DiaChiHienThi => DiaChiChiTiet ?? "Chưa có địa chỉ giao hàng";
        public List<GioHangVM> DanhSachSanPham { get; set; } = new();
        public List<DiaChiGiaoHang> DanhSachDiaChi { get; set; } = new();
        public List<Voucher> DanhSachVoucher { get; set; } = new();
        public double PhiVanChuyen { get; set; }
        public double GiamGiaVoucher { get; set; }
        public int? MaVoucherChon { get; set; }
        public DateTime NgayHeThong { get; set; }
        public double TongTienHang => (double)DanhSachSanPham.Sum(x => x.ThanhTien);
        public double TongThanhToan => TongTienHang - GiamGiaVoucher + PhiVanChuyen;
    }
}