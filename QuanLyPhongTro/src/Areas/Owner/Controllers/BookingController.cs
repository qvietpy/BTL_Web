using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;
        public BookingController(AppDbContext db){ _db=db; }
        private Guid? CurrentOwnerId(){ var id=User.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(id, out var g)? g : (Guid?)null; }

        // Chỉ hiển thị yêu cầu Pending để khi duyệt/ từ chối biến mất khỏi danh sách
        public async Task<IActionResult> Requests(){
            var oid = CurrentOwnerId();
            var items = await _db.BookingRequests
                .Include(b=>b.Room)
                .Include(b=>b.Renter).ThenInclude(r=>r.IdDetailNavigation)
                .Where(b=> (oid==null || b.Room.IdOwner==oid) && b.Status=="Pending")
                .OrderByDescending(b=>b.DateCreated)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(Guid id){
            var req = await _db.BookingRequests.Include(b=>b.Room).Include(b=>b.Renter).ThenInclude(r=>r.IdDetailNavigation)
                .FirstOrDefaultAsync(b=>b.Id==id);
            if(req==null) return NotFound();
            if(req.Status!="Pending") { TempData["ErrorMessage"]="Yêu cầu đã được xử lý."; return RedirectToAction(nameof(Requests)); }
            ViewBag.RoomName = req.Room?.Name;
            ViewBag.RenterName = req.Renter?.IdDetailNavigation?.Name ?? req.Renter?.Username;
            ViewBag.StartDate = req.DesiredStartDate.ToString("dd/MM/yyyy");
            ViewBag.Duration = req.DesiredDurationMonths;
            ViewBag.RoomPrice = (req.Room?.Price ?? 0m).ToString("N0");
            return View(model: req.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost(Guid id){
            var req = await _db.BookingRequests.Include(b=>b.Room).FirstOrDefaultAsync(b=>b.Id==id);
            if(req==null) return NotFound();
            if(req.Status!="Pending") { TempData["ErrorMessage"]="Yêu cầu đã xử lý."; return RedirectToAction(nameof(Requests)); }
            var start = DateOnly.FromDateTime(req.DesiredStartDate);
            var end = start.AddMonths(req.DesiredDurationMonths);
            var hasOverlap = await _db.Contracts.AnyAsync(c=> c.IdRoom==req.IdRoom && c.Status=="Active" && c.EndDate >= start && c.StartDate <= end);
            if(hasOverlap){ TempData["ErrorMessage"]="Phòng đã có hợp đồng trùng thời gian."; return RedirectToAction(nameof(Requests)); }
            var contract = new Contract{ IdRoom = req.IdRoom, IdRenter = req.IdRenter, StartDate = start, EndDate = end, Deposit = 0m, Status = "Active" };
            if(req.Room!=null) req.Room.Status = "Rented";
            _db.Contracts.Add(contract);
            // Xóa yêu cầu sau khi tạo hợp đồng để danh sách chỉ còn Pending
            _db.BookingRequests.Remove(req);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã tạo hợp đồng và xóa yêu cầu.";
            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id){
            var req = await _db.BookingRequests.FirstOrDefaultAsync(b=>b.Id==id);
            if(req==null) return NotFound();
            if(req.Status!="Pending"){ TempData["ErrorMessage"]="Yêu cầu đã xử lý."; return RedirectToAction(nameof(Requests)); }
            // Xóa khỏi DB thay vì đổi trạng thái để biến mất
            _db.BookingRequests.Remove(req);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã từ chối và xóa yêu cầu.";
            return RedirectToAction(nameof(Requests));
        }
    }
}
