namespace LibraryAPI.Models
{
    public class PhieuPhat
    {
        public int MaPhat { get; set; }
        public int MaPhieu { get; set; }
        public decimal TienPhat { get; set; }
        public string LyDo { get; set; } = null!;

        // Navigation
        public PhieuMuon PhieuMuon { get; set; } = null!;
    }
}