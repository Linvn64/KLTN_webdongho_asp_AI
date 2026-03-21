namespace WebDongHoLG.Areas.Admin.Models
{
    internal class PhanQuyenViewModel
    {
        public string UserId { get; set; }
        public string HoTen { get; set; }
        public List<string> QuyenHienTai { get; set; }
        public List<string> TatCaChucNang { get; set; }
    }
}