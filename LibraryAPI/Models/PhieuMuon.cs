namespace LibraryAPI.Models
{
    public class PhieuMuon
    {
        public int MaPhieu { get; set; }
        public string MaSV { get; set; } = null!;
        public DateTime NgayMuon { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime? NgayTra { get; set; }   // NULL = chưa trả
        public string TrangThai { get; set; } = null!;

        // Navigation
        public SinhVien SinhVien { get; set; } = null!;
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();
        public PhieuPhat? PhieuPhat { get; set; }
    }
}