using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options) { }

        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<PhieuMuon> PhieuMuons { get; set; }
        public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }
        public DbSet<PhieuPhat> PhieuPhats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── SinhVien ──────────────────────────────────────
            modelBuilder.Entity<SinhVien>(e =>
            {
                e.ToTable("SinhVien");
                e.HasKey(x => x.MaSV);
                e.Property(x => x.Email).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ── TheLoai ───────────────────────────────────────
            modelBuilder.Entity<TheLoai>(e =>
            {
                e.ToTable("TheLoai");
                e.HasKey(x => x.MaTheLoai);
            });

            // ── Sach ──────────────────────────────────────────
            modelBuilder.Entity<Sach>(e =>
            {
                e.ToTable("Sach");
                e.HasKey(x => x.MaSach);

                e.HasOne(x => x.TheLoai)
                 .WithMany(x => x.Sachs)
                 .HasForeignKey(x => x.MaTheLoai)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── PhieuMuon ─────────────────────────────────────
            modelBuilder.Entity<PhieuMuon>(e =>
            {
                e.ToTable("PhieuMuon");
                e.HasKey(x => x.MaPhieu);
                e.Property(x => x.NgayTra).IsRequired(false);

                e.HasOne(x => x.SinhVien)
                 .WithMany(x => x.PhieuMuons)
                 .HasForeignKey(x => x.MaSV)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ChiTietPhieuMuon ──────────────────────────────
            modelBuilder.Entity<ChiTietPhieuMuon>(e =>
            {
                e.ToTable("ChiTietPhieuMuon");
                e.HasKey(x => x.MaChiTiet);

                e.HasOne(x => x.PhieuMuon)
                 .WithMany(x => x.ChiTietPhieuMuons)
                 .HasForeignKey(x => x.MaPhieu)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Sach)
                 .WithMany(x => x.ChiTietPhieuMuons)
                 .HasForeignKey(x => x.MaSach)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── PhieuPhat ─────────────────────────────────────
            modelBuilder.Entity<PhieuPhat>(e =>
            {
                e.ToTable("PhieuPhat");
                e.HasKey(x => x.MaPhat);
                e.HasIndex(x => x.MaPhieu).IsUnique();

                e.HasOne(x => x.PhieuMuon)
                 .WithOne(x => x.PhieuPhat)
                 .HasForeignKey<PhieuPhat>(x => x.MaPhieu)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}