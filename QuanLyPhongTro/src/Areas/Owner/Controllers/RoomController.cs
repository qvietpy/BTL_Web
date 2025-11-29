using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;
        public RoomController(AppDbContext context){ _context = context; }

        private Guid? CurrentOwnerId(){ var id = User.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(id, out var g) ? g : (Guid?)null; }

        // GET: /Owner/Room
        public async Task<IActionResult> Index(){
            var oid = CurrentOwnerId();
            var rooms = await _context.Rooms
                .Include(r=>r.Contracts)
                .Include(r=>r.RoomImages)
                .Where(r => oid==null || r.IdOwner==oid)
                .OrderByDescending(r=>r.Id)
                .ToListAsync();
            return View(rooms);
        }

        [HttpGet]
        // [GET] Owner/Room/Detail/{id}
        public async Task<IActionResult> Detail(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomImages)      // 1. Lấy kèm hình ảnh
                .Include(r => r.Contracts)       // 2. Lấy kèm hợp đồng
                    .ThenInclude(c => c.IdRenterNavigation) // Lấy thông tin người thuê
                        .ThenInclude(u => u.IdDetailNavigation) // Lấy tên chi tiết người thuê
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: /Owner/Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomCreateInput input){
            if(!ModelState.IsValid) return RedirectToAction(nameof(Index));
            var oid = CurrentOwnerId();
            var room = new Room{ Name = input.Name, Address = input.Address, Area = input.Area, Price = input.Price, Description = input.Description, Status = "Empty", IdOwner = oid };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm phòng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomDetails(Guid id){
            var room = await _context.Rooms.FirstOrDefaultAsync(r=>r.Id==id);
            if(room==null) return NotFound();
            return Json(new { room.Id, room.Name, room.Address, room.Area, room.Price, room.Description, room.Status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomEditInput input){
            if(!ModelState.IsValid) return RedirectToAction(nameof(Index));
            var room = await _context.Rooms.FindAsync(input.Id);
            if(room==null) return NotFound();
            room.Name = input.Name; room.Address = input.Address; room.Area = input.Area; room.Price = input.Price; room.Description = input.Description; room.Status = input.Status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật phòng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id){
            var room = await _context.Rooms.Include(r=>r.Contracts).FirstOrDefaultAsync(r=>r.Id==id);
            if(room==null) return NotFound();
            if(room.Contracts.Any(c=>c.Status=="Active")){ TempData["ErrorMessage"] = "Không thể xóa phòng đang có hợp đồng."; return RedirectToAction(nameof(Index)); }
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa phòng.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class RoomCreateInput{
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal? Area { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
    }
    public class RoomEditInput{
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal? Area { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}