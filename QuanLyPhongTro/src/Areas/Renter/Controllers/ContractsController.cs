using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using QuanLyPhongTro.src.Models.ViewModels;

namespace QuanLyPhongTro.Areas.Renter.Controllers
{
    [Area("Renter")]
    public class ContractsController : Controller
    {
        private readonly AppDbContext _db;
        public ContractsController(AppDbContext db) { _db = db; }

        private Guid? CurrentPersonId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {
            var pid = CurrentPersonId();
            if (pid == null) return RedirectToAction("Login", "Auth", new { area = "Renter" });

            var items = await _db.Contracts
                .Include(c => c.IdRoomNavigation)!
                    .ThenInclude(r => r.IdOwnerNavigation)!
                        .ThenInclude(o => o.IdDetailNavigation)
                .Where(c => c.IdRenter == pid)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new RenterContractViewModel
                {
                    Id = c.Id,
                    RoomName = c.IdRoomNavigation!.Name,
                    RoomAddress = c.IdRoomNavigation!.Address!,
                    StartDate = c.StartDate.HasValue ? new DateTime(c.StartDate.Value.Year, c.StartDate.Value.Month, c.StartDate.Value.Day) : DateTime.MinValue,
                    EndDate = c.EndDate.HasValue ? new DateTime(c.EndDate.Value.Year, c.EndDate.Value.Month, c.EndDate.Value.Day) : DateTime.MinValue,
                    Deposit = c.Deposit ?? 0,
                    Status = c.Status ?? "Pending",
                    OwnerName = c.IdRoomNavigation!.IdOwnerNavigation!.IdDetailNavigation!.Name,
                    OwnerPhone = c.IdRoomNavigation!.IdOwnerNavigation!.IdDetailNavigation!.Phone!
                }).ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(Guid id, int months)
        {
            var pid = CurrentPersonId(); if(pid==null) return Unauthorized();
            if(months <1 || months >36){ TempData["ErrorMessage"]="Số tháng gia hạn phải từ 1-36"; return RedirectToAction("Contract","Dashboard"); }
            var c = await _db.Contracts.FirstOrDefaultAsync(x=>x.Id==id && x.IdRenter==pid && x.Status=="Active");
            if(c==null){ TempData["ErrorMessage"]="Không tìm thấy hợp đồng"; return RedirectToAction("Contract","Dashboard"); }
            if(c.ExtensionStatus=="Pending"){ TempData["ErrorMessage"]="Đã có yêu cầu gia hạn đang chờ duyệt. Vui lòng chờ chủ trọ duyệt xong."; return RedirectToAction("Contract","Dashboard"); }
            c.RequestedExtensionMonths = months;
            c.ExtensionStatus = "Pending";
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã gửi yêu cầu gia hạn";
            return RedirectToAction("Contract","Dashboard");
        }
    }
}
