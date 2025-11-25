using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        public ReportController(AppDbContext db){ _db=db; }

        public async Task<IActionResult> Index(){
            var items = await _db.Reports
                .Include(r=>r.IdRoomNavigation)
                .Include(r=>r.IdReporterNavigation).ThenInclude(p=>p.IdDetailNavigation)
                .OrderByDescending(r=>r.DateCreated)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInProgress(Guid id){
            var report = await _db.Reports.FindAsync(id); if(report==null) return NotFound();
            if(report.Status=="Pending"){ report.Status = "InProgress"; await _db.SaveChangesAsync(); TempData["SuccessMessage"]="?ã chuy?n sang x? lý."; }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkResolved(Guid id){
            var report = await _db.Reports.FindAsync(id); if(report==null) return NotFound();
            if(report.Status=="InProgress" || report.Status=="Pending"){ report.Status = "Resolved"; await _db.SaveChangesAsync(); TempData["SuccessMessage"]="?ã ?ánh d?u hoàn t?t."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
