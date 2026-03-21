namespace WebDongHoLG.Data
{
    public class ThuongHieu
    {
        
            public int Id { get; set; }
            public string TenThuongHieu { get; set; }

            public ICollection<SanPham> SanPhams { get; set; }
        
    }
}
