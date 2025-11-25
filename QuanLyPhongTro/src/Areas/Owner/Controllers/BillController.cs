using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class BillController : Controller
    {
        private readonly AppDbContext _context;
        public BillController(AppDbContext context){ _context = context; }
        private Guid? CurrentOwnerId(){ var id=User.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(id, out var g) ? g : (Guid?)null; }

        // GET: /Owner/Bill
        public async Task<IActionResult> Index()
        {
            var oid = CurrentOwnerId();
            var bills = await _context.Bills
                .Include(b => b.IdRoomNavigation)
                .Include(b => b.IdPersonNavigation)!.ThenInclude(p => p.IdDetailNavigation)
                .Include(b => b.BillDetails)
                .Where(b => oid==null || b.IdRoomNavigation!.IdOwner==oid)
                .OrderByDescending(b => b.DateCreated)
                .ToListAsync();
            return View(bills);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var oid = CurrentOwnerId();
            var rooms = await _context.Rooms.Where(r => oid==null || r.IdOwner==oid).ToListAsync();
            var renters = await _context.Contracts.Include(c=>c.IdRenterNavigation)!.ThenInclude(p=>p.IdDetailNavigation)
                             .Where(c=>c.Status=="Active" && (oid==null || c.IdRoomNavigation!.IdOwner==oid))
                             .Select(c=>c.IdRenterNavigation!).Distinct().ToListAsync();
            ViewBag.Rooms = rooms;
            ViewBag.Renters = renters;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid roomId, Guid renterId, decimal totalMoney)
        {
            if(totalMoney < 0){ ModelState.AddModelError("totalMoney","Tổng tiền không hợp lệ"); }
            var room = await _context.Rooms.FindAsync(roomId); var renter = await _context.People.FindAsync(renterId);
            if(room==null || renter==null){ ModelState.AddModelError("roomId","Phòng hoặc người thuê không tồn tại"); }
            if(!ModelState.IsValid){ return await Create(); }
            var bill = new Bill{ Id = Guid.NewGuid(), IdRoom = room.Id, IdPerson = renter.Id, TotalMoney = totalMoney, Status = "Chưa thanh toán", DateCreated = DateTime.UtcNow };
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã tạo hóa đơn.";
            return RedirectToAction(nameof(Index));
        }

        // Auto generate bills for all active contracts of this owner (one per contract, current month)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAll()
        {
            var oid = CurrentOwnerId(); if(oid==null){ TempData["ErrorMessage"]="Không xác định chủ trọ"; return RedirectToAction(nameof(Index)); }
            var now = DateTime.UtcNow;
            // avoid duplicates: same room & renter & month
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var existing = await _context.Bills.Where(b=> b.DateCreated>=monthStart && b.DateCreated < monthStart.AddMonths(1) && b.IdRoomNavigation!=null && b.IdRoomNavigation.IdOwner==oid).Select(b=> new { b.IdRoom, b.IdPerson}).ToListAsync();
            var existingSet = existing.Select(e=> (e.IdRoom, e.IdPerson)).ToHashSet();
            var contracts = await _context.Contracts.Include(c=>c.IdRoomNavigation)
                              .Where(c=> c.Status=="Active" && c.IdRoomNavigation!=null && c.IdRoomNavigation.IdOwner==oid).ToListAsync();
            int created=0;
            foreach(var c in contracts){
                if(c.IdRoom==null || c.IdRenter==null) continue;
                if(existingSet.Contains((c.IdRoom.Value, c.IdRenter.Value))) continue;
                var price = c.IdRoomNavigation?.Price ?? 0m;
                var bill = new Bill{ Id=Guid.NewGuid(), IdRoom=c.IdRoom, IdPerson=c.IdRenter, TotalMoney=price, Status="Chưa thanh toán", DateCreated=DateTime.UtcNow };
                _context.Bills.Add(bill); created++;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"]=$"Đã tạo {created} hóa đơn cho tháng này.";
            return RedirectToAction(nameof(Index));
        }

        // Mark all unpaid bills of this owner as 'Pending' send state (optional) - or just leave status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAll()
        {
            var oid = CurrentOwnerId(); if(oid==null){ TempData["ErrorMessage"]="Không xác định chủ trọ"; return RedirectToAction(nameof(Index)); }
            var bills = await _context.Bills.Include(b=>b.IdRoomNavigation)
                           .Where(b=> b.Status=="Chưa thanh toán" && b.IdRoomNavigation!=null && b.IdRoomNavigation.IdOwner==oid)
                           .ToListAsync();
            foreach(var b in bills){ b.Status = "Pending"; }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"]=$"Đã gửi {bills.Count} hóa đơn cho người thuê.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            var bill = await _context.Bills.FindAsync(id); if(bill==null) return NotFound();
            bill.Status = "Đã thanh toán"; bill.PaymentDate = DateTime.UtcNow; await _context.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã cập nhật trạng thái hóa đơn.";
            return RedirectToAction(nameof(Index));
        }
    }
}