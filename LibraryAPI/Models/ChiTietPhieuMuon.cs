namespace LibraryAPI.Models
{
    public class ChiTietPhieuMuon
    {
        public int MaChiTiet { get; set; }
        public int MaPhieu { get; set; }
        public string MaSach { get; set; } = null!;
        public int SoLuong { get; set; }

        // Navigation
        public PhieuMuon PhieuMuon { get; set; } = null!;
        public Sach Sach { get; set; } = null!;
    }
}