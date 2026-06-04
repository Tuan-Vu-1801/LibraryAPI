using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheLoaiController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public TheLoaiController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET /api/theloai
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.TheLoais.ToListAsync();
            return Ok(list);
        }

        // GET /api/theloai/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai == null)
                return NotFound(new { message = $"Không tìm thấy thể loại {id}" });

            return Ok(theLoai);
        }

        // POST /api/theloai
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TheLoai theLoai)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.TheLoais.AnyAsync(t => t.MaTheLoai == theLoai.MaTheLoai))
                return BadRequest(new { message = "Mã thể loại đã tồn tại" });

            _context.TheLoais.Add(theLoai);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = theLoai.MaTheLoai }, theLoai);
        }

        // PUT /api/theloai/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TheLoai theLoai)
        {
            if (id != theLoai.MaTheLoai)
                return BadRequest(new { message = "MaTheLoai không khớp" });

            var existing = await _context.TheLoais.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Không tìm thấy thể loại {id}" });

            existing.TenTheLoai = theLoai.TenTheLoai;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE /api/theloai/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai == null)
                return NotFound(new { message = $"Không tìm thấy thể loại {id}" });

            // Không xóa nếu còn sách thuộc thể loại này
            if (await _context.Sachs.AnyAsync(s => s.MaTheLoai == id))
                return BadRequest(new { message = "Không thể xóa thể loại còn chứa sách" });

            _context.TheLoais.Remove(theLoai);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }
    }
}