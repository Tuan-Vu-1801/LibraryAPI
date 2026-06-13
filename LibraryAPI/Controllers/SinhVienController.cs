using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SinhVienController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public SinhVienController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET /api/sinhvien
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.SinhViens.ToListAsync();
            return Ok(list);
        }

        // GET /api/sinhvien/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var sv = await _context.SinhViens
                .Include(s => s.PhieuMuons)
                .FirstOrDefaultAsync(s => s.MaSV == id);

            if (sv == null)
                return NotFound(new { message = $"Không tìm thấy sinh viên {id}" });

            return Ok(sv);
        }

        // POST /api/sinhvien
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SinhVien sv)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.SinhViens.AnyAsync(s => s.MaSV == sv.MaSV))
                return BadRequest(new { message = "Mã sinh viên đã tồn tại" });

            if (await _context.SinhViens.AnyAsync(s => s.Email == sv.Email))
                return BadRequest(new { message = "Email đã được sử dụng" });

            _context.SinhViens.Add(sv);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = sv.MaSV }, sv);
        }

        // PUT /api/sinhvien/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SinhVien sv)
        {
            if (id != sv.MaSV)
                return BadRequest(new { message = "MaSV không khớp" });

            var existing = await _context.SinhViens.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Không tìm thấy sinh viên {id}" });

            // Kiểm tra email trùng với sv khác
            if (await _context.SinhViens.AnyAsync(s => s.Email == sv.Email && s.MaSV != id))
                return BadRequest(new { message = "Email đã được sử dụng bởi sinh viên khác" });

            existing.HoTen = sv.HoTen;
            existing.Lop = sv.Lop;
            existing.Email = sv.Email;
            existing.SoDienThoai = sv.SoDienThoai;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        // Add advand crud - your task
        // DELETE /api/sinhvien/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var sv = await _context.SinhViens.FindAsync(id);
            if (sv == null)
                return NotFound(new { message = $"Không tìm thấy sinh viên {id}" });

            // Không xóa nếu còn phiếu mượn
            if (await _context.PhieuMuons.AnyAsync(p => p.MaSV == id))
                return BadRequest(new { message = "Không thể xóa sinh viên còn lịch sử mượn sách" });

            _context.SinhViens.Remove(sv);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }
    }
}