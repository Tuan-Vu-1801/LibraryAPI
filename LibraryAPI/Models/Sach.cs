namespace LibraryAPI.Models
{
    public class Sach
    {
        public string MaSach { get; set; } = null!;
        public string TenSach { get; set; } = null!;
        public string TacGia { get; set; } = null!;
        public int MaTheLoai { get; set; }
        public int SoLuongTon { get; set; }

        // Navigation
        public TheLoai TheLoai { get; set; } = null!;
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();
    }
}