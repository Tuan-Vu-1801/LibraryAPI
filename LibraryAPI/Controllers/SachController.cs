using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SachController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public SachController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET /api/sach?maTheLoai=1&search=lap trinh
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? maTheLoai,
            [FromQuery] string? search)
        {
            var query = _context.Sachs
                .Include(s => s.TheLoai)
                .AsQueryable();

            if (maTheLoai.HasValue)
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.TenSach.Contains(search));

            var result = await query.ToListAsync();
            return Ok(result);
        }

        // GET /api/sach/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
                return NotFound(new { message = $"Không tìm thấy sách {id}" });

            return Ok(sach);
        }

        // POST /api/sach
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sach sach)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra MaSach đã tồn tại chưa
            if (await _context.Sachs.AnyAsync(s => s.MaSach == sach.MaSach))
                return BadRequest(new { message = "Mã sách đã tồn tại" });

            // Kiểm tra TheLoai hợp lệ
            if (!await _context.TheLoais.AnyAsync(t => t.MaTheLoai == sach.MaTheLoai))
                return BadRequest(new { message = "Thể loại không tồn tại" });

            _context.Sachs.Add(sach);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = sach.MaSach }, sach);
        }

        // PUT /api/sach/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Sach sach)
        {
            if (id != sach.MaSach)
                return BadRequest(new { message = "MaSach không khớp" });

            var existing = await _context.Sachs.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Không tìm thấy sách {id}" });

            existing.TenSach = sach.TenSach;
            existing.TacGia = sach.TacGia;
            existing.MaTheLoai = sach.MaTheLoai;
            existing.SoLuongTon = sach.SoLuongTon;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE /api/sach/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null)
                return NotFound(new { message = $"Không tìm thấy sách {id}" });

            // Không xóa nếu đang có trong phiếu mượn
            if (await _context.ChiTietPhieuMuons.AnyAsync(c => c.MaSach == id))
                return BadRequest(new { message = "Không thể xóa sách đang có trong phiếu mượn" });

            _context.Sachs.Remove(sach);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }
    }
}