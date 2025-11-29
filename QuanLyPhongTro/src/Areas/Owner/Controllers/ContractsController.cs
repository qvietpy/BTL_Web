using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ContractsController : Controller
    {
        private readonly AppDbContext _db;
        public ContractsController(AppDbContext db){ _db=db; }

        public async Task<IActionResult> Index(){
            var items = await _db.Contracts
                .Include(c=>c.IdRoomNavigation)
                .Include(c=>c.IdRenterNavigation).ThenInclude(p=>p.IdDetailNavigation)
                .OrderByDescending(c=>c.StartDate)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveExtension(Guid id){
            var c = await _db.Contracts.FirstOrDefaultAsync(x=>x.Id==id);
            if(c==null){ TempData["ErrorMessage"]="Không tìm thấy hợp đồng"; return RedirectToAction(nameof(Index)); }
            if(!(c.ExtensionStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) ?? false) || c.RequestedExtensionMonths==null){ TempData["ErrorMessage"]="Không có yêu cầu gia hạn"; return RedirectToAction(nameof(Index)); }
            if(c.EndDate.HasValue){ c.EndDate = c.EndDate.Value.AddMonths(c.RequestedExtensionMonths.Value); }
            c.ExtensionStatus = "Approved"; c.RequestedExtensionMonths = null; // reset after apply
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã duyệt yêu cầu gia hạn hợp đồng";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectExtension(Guid id){
            var c = await _db.Contracts.FirstOrDefaultAsync(x=>x.Id==id);
            if(c==null){ TempData["ErrorMessage"]="Không tìm thấy hợp đồng"; return RedirectToAction(nameof(Index)); }
            if(!(c.ExtensionStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) ?? false)){ TempData["ErrorMessage"]="Không có yêu c?u gia h?n"; return RedirectToAction(nameof(Index)); }
            c.ExtensionStatus = "Rejected"; c.RequestedExtensionMonths = null;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"]="Đã từ chối yêu cầu gia hạn";
            return RedirectToAction(nameof(Index));
        }
    }
}
