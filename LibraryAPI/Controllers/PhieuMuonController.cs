using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhieuMuonController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public PhieuMuonController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.PhieuMuons
                .Include(p => p.SinhVien)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .Include(p => p.PhieuPhat)
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var phieu = await _context.PhieuMuons
                .Include(p => p.SinhVien)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .Include(p => p.PhieuPhat)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
                return NotFound(new { message = $"Không tìm thấy phiếu mượn {id}" });

            return Ok(phieu);
        }

        [HttpGet("sinhvien/{maSV}")]
        public async Task<IActionResult> GetBySinhVien(string maSV)
        {
            var list = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.Sach)
                .Include(p => p.PhieuPhat)
                .Where(p => p.MaSV == maSV)
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> TaoPhieuMuon([FromBody] TaoPhieuMuonRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sv = await _context.SinhViens.FindAsync(request.MaSV);
            if (sv == null)
                return NotFound(new { message = "Sinh viên không tồn tại" });

            var soSachDangMuon = await _context.ChiTietPhieuMuons
                .Where(c => c.PhieuMuon.MaSV == request.MaSV
                         && c.PhieuMuon.TrangThai == "DangMuon")
                .SumAsync(c => c.SoLuong);

            var soSachMuonMoi = request.ChiTiets.Sum(c => c.SoLuong);

            if (soSachDangMuon + soSachMuonMoi > 5)
                return BadRequest(new
                {
                    message = $"Sinh viên đang giữ {soSachDangMuon} sách. " +
                              $"Không thể mượn thêm {soSachMuonMoi} sách (tối đa 5)."
                });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.ChiTiets)
                {
                    var sach = await _context.Sachs
                        .FirstOrDefaultAsync(s => s.MaSach == item.MaSach);

                    if (sach == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { message = $"Sách {item.MaSach} không tồn tại" });
                    }

                    if (sach.SoLuongTon < item.SoLuong)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            message = $"Sách '{sach.TenSach}' chỉ còn {sach.SoLuongTon} cuốn, " +
                                      $"không đủ {item.SoLuong} cuốn yêu cầu."
                        });
                    }
                }

                var phieuMuon = new PhieuMuon
                {
                    MaSV = request.MaSV,
                    NgayMuon = DateTime.Today,
                    NgayHenTra = request.NgayHenTra,
                    NgayTra = null,
                    TrangThai = "DangMuon"
                };
                _context.PhieuMuons.Add(phieuMuon);
                await _context.SaveChangesAsync();

                foreach (var item in request.ChiTiets)
                {
                    var sach = await _context.Sachs.FindAsync(item.MaSach);
                    sach!.SoLuongTon -= item.SoLuong;

                    _context.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                    {
                        MaPhieu = phieuMuon.MaPhieu,
                        MaSach = item.MaSach,
                        SoLuong = item.SoLuong
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById),
                    new { id = phieuMuon.MaPhieu }, phieuMuon);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi server", detail = ex.Message });
            }
        }

        [HttpPut("{id}/tra")]
        public async Task<IActionResult> TraSach(int id)
        {
            var phieu = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
                return NotFound(new { message = $"Không tìm thấy phiếu mượn {id}" });

            if (phieu.TrangThai == "DaTra")
                return BadRequest(new { message = "Phiếu này đã được trả trước đó" });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                phieu.NgayTra = DateTime.Today;
                phieu.TrangThai = "DaTra";

                foreach (var chiTiet in phieu.ChiTietPhieuMuons)
                {
                    var sach = await _context.Sachs.FindAsync(chiTiet.MaSach);
                    if (sach != null)
                        sach.SoLuongTon += chiTiet.SoLuong;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Trả sách thành công",
                    maPhieu = phieu.MaPhieu,
                    ngayTra = phieu.NgayTra,
                    treHan = phieu.NgayTra > phieu.NgayHenTra
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi server", detail = ex.Message });
            }
        }

        [HttpGet("/api/dashboard/top5sach")]
        public async Task<IActionResult> Top5Sach()
        {
            var thang = DateTime.Now.Month;
            var nam = DateTime.Now.Year;

            var result = await _context.ChiTietPhieuMuons
                .Include(ct => ct.PhieuMuon)
                .Include(ct => ct.Sach)
                .Where(ct => ct.PhieuMuon.NgayMuon.Month == thang
                          && ct.PhieuMuon.NgayMuon.Year == nam)
                .GroupBy(ct => new { ct.MaSach, ct.Sach.TenSach })
                .Select(g => new
                {
                    tenSach = g.Key.TenSach,
                    soLanMuon = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.soLanMuon)
                .Take(5)
                .ToListAsync();

            return Ok(result);
        }
    }

    public class TaoPhieuMuonRequest
    {
        public string MaSV { get; set; } = null!;
        public DateTime NgayHenTra { get; set; }
        public List<ChiTietRequest> ChiTiets { get; set; } = new();
    }

    public class ChiTietRequest
    {
        public string MaSach { get; set; } = null!;
        public int SoLuong { get; set; }
    }
}