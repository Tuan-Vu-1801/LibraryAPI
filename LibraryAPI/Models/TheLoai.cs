namespace LibraryAPI.Models
{
    public class TheLoai
    {
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; } = null!;

        // Navigation
        public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
    }
}