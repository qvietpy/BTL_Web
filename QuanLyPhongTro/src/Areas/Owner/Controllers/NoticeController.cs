using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class NoticeController : Controller
    {
        private readonly AppDbContext _db;
        public NoticeController(AppDbContext db){ _db=db; }

        public async Task<IActionResult> Index(){
            var items = await _db.Notices.OrderByDescending(n=>n.CreatedAt).ToListAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult Create(){ return View(new Notice()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice){
            if(!ModelState.IsValid) return View(notice);
            notice.Id = Guid.NewGuid();
            notice.CreatedAt = DateTime.UtcNow;
            notice.Status = "Ch?a ??c";
            _db.Notices.Add(notice);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "?ã t?o thông báo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(Guid id){
            var item = await _db.Notices.FindAsync(id); if(item==null) return NotFound();
            item.Status = "?ã ??c"; await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
