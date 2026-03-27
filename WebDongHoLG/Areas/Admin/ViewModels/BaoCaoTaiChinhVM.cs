namespace WebDongHoLG.Areas.Admin.ViewModels
{
    public class BaoCaoTaiChinhVM
    {
        // Nhóm 1: Vốn hóa kho hàng
        public decimal TongVonTonKho { get; set; }  
        public int TongSoLuongTon { get; set; }     
        public decimal GiaTriBanRaDuKien { get; set; } 

        // Nhóm 2: Dòng tiền thực tế (Tháng này)
        public decimal TienVao { get; set; }   
        public decimal TienRa { get; set; }    
        public decimal LoiNhuanGop { get; set; }
        public string KieuLoc { get; set; }
        public int SelectedThang { get; set; }
        public int SelectedNam { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        // Nhóm 3: Cảnh báo
        public int SanPhamSapHetHang { get; set; }

        public List<decimal> BieuDoTienVao { get; set; } = new List<decimal>();
        public List<decimal> BieuDoTienRa { get; set; } = new List<decimal>();
        public List<string> DanhSachNgay { get; set; } = new List<string>();
    }
}