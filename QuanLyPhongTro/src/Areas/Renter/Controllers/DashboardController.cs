using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.src.Models.ViewModels;
using QuanLyPhongTro.Models; // added
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace QuanLyPhongTro.Areas.Renter.Controllers
{
    [Area("Renter")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db){ _db = db; }

        private Guid? CurrentPersonId(){ var id = User.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(id, out var g) ? g : (Guid?)null; }

        public async Task<IActionResult> Index(){
            var pid = CurrentPersonId();
            if(pid == null) return RedirectToAction("Login","Auth", new { area="Renter" });
            var contract = await _db.Contracts.Include(c=>c.IdRoomNavigation).FirstOrDefaultAsync(c=>c.IdRenter==pid && c.Status=="Active");
            if(contract == null){ return View("NoRoom"); }
            var unpaidBills = await _db.Bills.Where(b=>b.IdPerson==pid && (b.Status=="Chưa thanh toán" || b.Status=="Pending") ).ToListAsync();
            var vm = new RenterDashboardViewModel{
                ContractId = contract.Id,
                RoomId = contract.IdRoom ?? Guid.Empty,
                RoomName = contract.IdRoomNavigation?.Name ?? "",
                RoomAddress = contract.IdRoomNavigation?.Address,
                RoomPrice = contract.IdRoomNavigation?.Price,
                RoomArea = contract.IdRoomNavigation?.Area,
                RoomStatus = contract.IdRoomNavigation?.Status,
                ContractStart = contract.StartDate,
                ContractEnd = contract.EndDate,
                Deposit = contract.Deposit,
                ContractStatus = contract.Status ?? "",
                OutstandingAmount = unpaidBills.Sum(b=>b.TotalMoney ?? 0m),
                UnpaidBills = unpaidBills.Select(b=> new BillItem{ Id=b.Id, Amount=b.TotalMoney ?? 0m, Status=b.Status }).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayAll(){
            var pid = CurrentPersonId();
            if(pid == null) return Unauthorized();
            var bills = await _db.Bills.Where(b=>b.IdPerson==pid && (b.Status=="Chưa thanh toán" || b.Status=="Pending")).ToListAsync();
            if(!bills.Any()) return RedirectToAction(nameof(Index));
            foreach(var b in bills){ b.Status = "Đã thanh toán"; b.PaymentDate = DateTime.UtcNow; }
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thanh toán thành công";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Contract(){
            var pid = CurrentPersonId(); if(pid==null) return RedirectToAction("Login","Auth", new { area="Renter" });
            var c = await _db.Contracts
                .Include(x=>x.IdRoomNavigation)!.ThenInclude(r=>r.IdOwnerNavigation)!.ThenInclude(o=>o.IdDetailNavigation)
                .FirstOrDefaultAsync(x=>x.IdRenter==pid && x.Status=="Active");
            if(c==null) return RedirectToAction(nameof(Index));
            var vm = new ContractDetailViewModel{
                Id = c.Id,
                RoomName = c.IdRoomNavigation?.Name ?? "",
                RoomAddress = c.IdRoomNavigation?.Address,
                Price = c.IdRoomNavigation?.Price,
                Area = c.IdRoomNavigation?.Area,
                Start = c.StartDate,
                End = c.EndDate,
                Deposit = c.Deposit,
                Status = c.Status ?? "",
                OwnerName = c.IdRoomNavigation?.IdOwnerNavigation?.IdDetailNavigation?.Name ?? c.IdRoomNavigation?.IdOwnerNavigation?.Username ?? "",
                OwnerPhone = c.IdRoomNavigation?.IdOwnerNavigation?.IdDetailNavigation?.Phone ?? ""
            };
            return View(vm);
        }

        public async Task<IActionResult> Profile(){
            var pid = CurrentPersonId(); if(pid==null) return RedirectToAction("Login","Auth", new { area="Renter" });
            var person = await _db.People.Include(p=>p.IdDetailNavigation).FirstOrDefaultAsync(p=>p.Id==pid);
            if(person==null) return NotFound();
            var vm = new PersonalInfoViewModel{ Username = person.Username, Name = person.IdDetailNavigation?.Name, Cccd = person.IdDetailNavigation?.Cccd, Phone = person.IdDetailNavigation?.Phone, Gender = person.IdDetailNavigation?.Gender, Gmail = person.IdDetailNavigation?.Gmail };
            return View(vm);
        }

        [HttpGet]
        public IActionResult SendNotice(){ return View(new SendNoticeInput()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotice(SendNoticeInput input){
            if(!ModelState.IsValid) return View(input);
            var pid = CurrentPersonId(); if(pid==null) return RedirectToAction("Login","Auth", new { area="Renter" });
            var contract = await _db.Contracts.Include(c=>c.IdRoomNavigation).FirstOrDefaultAsync(c=>c.IdRenter==pid && c.Status=="Active");
            if(contract==null) return RedirectToAction(nameof(Index));
            var report = new Report{ IdReporter = pid, IdRoom = contract.IdRoom, Title = input.Title, Description = input.Description, DateCreated = DateTime.UtcNow, Status = "Pending" };
            _db.Reports.Add(report);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã gửi thông báo/ báo cáo đến chủ trọ";
            return RedirectToAction(nameof(Index));
        }
    }
}
