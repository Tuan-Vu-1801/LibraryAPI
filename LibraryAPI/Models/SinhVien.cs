namespace LibraryAPI.Models
{
    public class SinhVien
    {
        public string MaSV { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string Lop { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;

        // Navigation
        public ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();
    }
}