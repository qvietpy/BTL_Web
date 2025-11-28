using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Areas.Owner.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class BillController : Controller
    {
        private readonly AppDbContext _context;

        public BillController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? CurrentOwnerId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        // GET: /Owner/Bill
        public async Task<IActionResult> Index()
        {
            var oid = CurrentOwnerId();
            var bills = await _context.Bills
                .Include(b => b.IdRoomNavigation)
                .Include(b => b.IdPersonNavigation)!.ThenInclude(p => p.IdDetailNavigation)
                .Include(b => b.BillDetails)
                .Where(b => oid == null || b.IdRoomNavigation!.IdOwner == oid)
                .OrderByDescending(b => b.DateCreated)
                .ToListAsync();

            return View(bills);
        }

        // GET: /Owner/Bill/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var oid = CurrentOwnerId();

            var contracts = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                .Include(c => c.IdRenterNavigation)!.ThenInclude(p => p.IdDetailNavigation)
                .Where(c => c.Status == "Active" && (oid == null || c.IdRoomNavigation!.IdOwner == oid))
                .ToListAsync();

            // Build select list items
            ViewBag.ContractSelect = contracts
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{(c.IdRoomNavigation?.Name ?? "Phòng")} - " +
                           $"{(c.IdRenterNavigation?.IdDetailNavigation?.Name ?? c.IdRenterNavigation?.Username ?? "Khách")}"
                })
                .Prepend(new SelectListItem { Value = "", Text = "Chọn hợp đồng..." })
                .ToList();

            // JSON for client-side autofill
            var contractJsonData = contracts.Select(c => new
            {
                id = c.Id,
                roomId = c.IdRoom,
                roomName = c.IdRoomNavigation?.Name,
                renterId = c.IdRenter,
                renterName = c.IdRenterNavigation?.IdDetailNavigation?.Name ?? c.IdRenterNavigation?.Username,
                rent = c.IdRoomNavigation?.Price ?? 0m
            }).ToList();
            ViewBag.ContractsJson = JsonSerializer.Serialize(contractJsonData);

            var vm = new InvoiceCreateViewModel1
            {
                InvoiceCode = $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                Month = DateTime.Today.Month,
                Year = DateTime.Today.Year,
                Rent = 0,
                Electricity = 0,
                Water = 0,
                ParkingFee = 0,
                GarbageFee = 0,
                DueDate = DateTime.Today.AddDays(7)
            };

            return View(vm);
        }

        // POST: /Owner/Bill/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateViewModel1 model)
        {
            var oid = CurrentOwnerId();

            // Repopulate select list + JSON if validation fails
            var contracts = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                .Include(c => c.IdRenterNavigation)!.ThenInclude(p => p.IdDetailNavigation)
                .Where(c => c.Status == "Active" && (oid == null || c.IdRoomNavigation!.IdOwner == oid))
                .ToListAsync();

            ViewBag.ContractSelect = contracts
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{(c.IdRoomNavigation?.Name ?? "Phòng")} - " +
                           $"{(c.IdRenterNavigation?.IdDetailNavigation?.Name ?? c.IdRenterNavigation?.Username ?? "Khách")}"
                })
                .Prepend(new SelectListItem { Value = "", Text = "Chọn hợp đồng..." })
                .ToList();

            var contractJsonData = contracts.Select(c => new
            {
                id = c.Id,
                roomId = c.IdRoom,
                roomName = c.IdRoomNavigation?.Name,
                renterId = c.IdRenter,
                renterName = c.IdRenterNavigation?.IdDetailNavigation?.Name ?? c.IdRenterNavigation?.Username,
                rent = c.IdRoomNavigation?.Price ?? 0m
            }).ToList();
            ViewBag.ContractsJson = JsonSerializer.Serialize(contractJsonData);

            if (!ModelState.IsValid)
                return View(model);

            if (model.ContractId == null)
            {
                ModelState.AddModelError(nameof(model.ContractId), "Vui lòng chọn hợp đồng.");
                return View(model);
            }

            var contract = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                .Include(c => c.IdRenterNavigation)!.ThenInclude(p => p.IdDetailNavigation)
                .FirstOrDefaultAsync(c => c.Id == model.ContractId && (oid == null || (c.IdRoomNavigation != null && c.IdRoomNavigation.IdOwner == oid)));

            if (contract == null)
            {
                ModelState.AddModelError(nameof(model.ContractId), "Hợp đồng không tồn tại hoặc không thuộc về chủ trọ hiện tại.");
                return View(model);
            }

            var roomId = contract.IdRoom ?? model.RoomId;
            var renterId = contract.IdRenter ?? model.RenterId;

            if (roomId == null || renterId == null)
            {
                ModelState.AddModelError(string.Empty, "Không xác định được phòng hoặc khách thuê từ hợp đồng.");
                return View(model);
            }

            var room = await _context.Rooms.FindAsync(roomId.Value);
            var renter = await _context.People.FindAsync(renterId.Value);

            if (room == null || renter == null)
            {
                ModelState.AddModelError(string.Empty, "Phòng hoặc người thuê không tồn tại.");
                return View(model);
            }

            decimal total = model.Rent + model.Electricity + model.Water + model.Services;
            if (total <= 0)
            {
                ModelState.AddModelError(string.Empty, "Tổng tiền phải lớn hơn 0.");
                return View(model);
            }

            var bill = new Bill
            {
                Id = Guid.NewGuid(),
                IdRoom = room.Id,
                IdPerson = renter.Id,
                TotalMoney = total,
                Status = "Chưa thanh toán",
                DateCreated = DateTime.UtcNow
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo hóa đơn thành công";
            return RedirectToAction(nameof(Index));
        }

        // POST: Generate bills for all active contracts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAll()
        {
            var oid = CurrentOwnerId();
            if (oid == null)
            {
                TempData["ErrorMessage"] = "Không xác định chủ trọ";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var existing = await _context.Bills
                .Where(b => b.DateCreated >= monthStart && b.DateCreated < monthStart.AddMonths(1) &&
                            b.IdRoomNavigation != null && b.IdRoomNavigation.IdOwner == oid)
                .Select(b => new { b.IdRoom, b.IdPerson })
                .ToListAsync();

            var existingSet = existing.Select(e => (e.IdRoom, e.IdPerson)).ToHashSet();

            var contracts = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                .Where(c => c.Status == "Active" && c.IdRoomNavigation != null && c.IdRoomNavigation.IdOwner == oid)
                .ToListAsync();

            int created = 0;
            foreach (var c in contracts)
            {
                if (c.IdRoom == null || c.IdRenter == null) continue;
                if (existingSet.Contains((c.IdRoom.Value, c.IdRenter.Value))) continue;

                var price = c.IdRoomNavigation?.Price ?? 0m;
                var bill = new Bill
                {
                    Id = Guid.NewGuid(),
                    IdRoom = c.IdRoom,
                    IdPerson = c.IdRenter,
                    TotalMoney = price,
                    Status = "Chưa thanh toán",
                    DateCreated = DateTime.UtcNow
                };
                _context.Bills.Add(bill);
                created++;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã tạo {created} hóa đơn cho tháng này.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Mark all unpaid bills as Pending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAll()
        {
            var oid = CurrentOwnerId();
            if (oid == null)
            {
                TempData["ErrorMessage"] = "Không xác định chủ trọ";
                return RedirectToAction(nameof(Index));
            }

            var bills = await _context.Bills
                .Include(b => b.IdRoomNavigation)
                .Where(b => b.Status == "Chưa thanh toán" && b.IdRoomNavigation != null && b.IdRoomNavigation.IdOwner == oid)
                .ToListAsync();

            foreach (var b in bills)
            {
                b.Status = "Pending";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã gửi {bills.Count} hóa đơn cho người thuê.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Mark a bill as paid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            bill.Status = "Đã thanh toán";
            bill.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái hóa đơn.";

            return RedirectToAction(nameof(Index));
        }
    }
}
