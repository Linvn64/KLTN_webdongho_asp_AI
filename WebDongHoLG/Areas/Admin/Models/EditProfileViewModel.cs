namespace WebDongHoLG.Areas.Admin.Models
{
    public class EditProfileViewModel
    {
        public string HoTen { get; set; }
        public string Sdt { get; set; }
        public string GioiTinh { get; set; }
        public string Email { get; set; } 
        public string AnhDaiDien { get; set; } 
        public IFormFile UploadAnh { get; set; }
    }
}
