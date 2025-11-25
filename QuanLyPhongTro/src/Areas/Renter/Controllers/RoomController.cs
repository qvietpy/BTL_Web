using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using QuanLyPhongTro.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using QuanLyPhongTro.src.Models.ViewModels;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.Renter.Controllers
{
    [Area("Renter")]
    public class RoomController : Controller
    {
        private readonly AppDbContext _db;
        public RoomController(AppDbContext db) { _db = db; }

        private Guid? GetCurrentPersonId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }

        // Toggle favorite (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] Guid roomId)
        {
            var personId = GetCurrentPersonId();
            if (personId == null) return Unauthorized();
            var existing = await _db.FavoriteRooms.FirstOrDefaultAsync(f => f.PersonId == personId && f.RoomId == roomId);
            if (existing != null)
            {
                _db.FavoriteRooms.Remove(existing);
                await _db.SaveChangesAsync();
                return Json(new { favorited = false });
            }
            _db.FavoriteRooms.Add(new FavoriteRoom { Id = Guid.NewGuid(), PersonId = personId, RoomId = roomId });
            await _db.SaveChangesAsync();
            return Json(new { favorited = true });
        }

        // List favorites for current user (AJAX)
        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var personId = GetCurrentPersonId();
            if (personId == null) return Unauthorized();
            var rooms = await _db.FavoriteRooms
                .Include(f => f.Room).ThenInclude(r => r.RoomImages)
                .Where(f => f.PersonId == personId)
                .Select(f => new
                {
                    id = f.Room.Id,
                    name = f.Room.Name,
                    price = f.Room.Price,
                    area = f.Room.Area,
                    image = f.Room.RoomImages.OrderBy(i => i.ImageUrl).FirstOrDefault().ImageUrl,
                    address = f.Room.Address
                }).ToListAsync();
            return Json(new { items = rooms });
        }

        // Room detail page
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var room = await _db.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.IdOwnerNavigation).ThenInclude(o => o.IdDetailNavigation)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();
            var vm = new RoomDetailViewModel
            {
                Id = room.Id,
                Name = room.Name,
                Address = room.Address ?? string.Empty,
                Area = room.Area ?? 0,
                Price = room.Price ?? 0,
                Status = room.Status ?? string.Empty,
                Description = room.Description ?? string.Empty,
                OwnerName = room.IdOwnerNavigation?.IdDetailNavigation?.Name ?? room.IdOwnerNavigation?.Username ?? "Chủ trọ",
                OwnerPhone = room.IdOwnerNavigation?.IdDetailNavigation?.Phone ?? string.Empty,
                OwnerEmail = room.IdOwnerNavigation?.IdDetailNavigation?.Gmail ?? string.Empty,
                ImageUrls = room.RoomImages.Select(i => i.ImageUrl!).OrderBy(s => s).ToList(),
                Services = new List<ServiceInfo>()
            };
            return View(vm);
        }

        // Create booking request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(Guid roomId, DateTime desiredStartDate, int desiredDurationMonths, string? note)
        {
            var personId = GetCurrentPersonId();
            if (personId == null)
                return RedirectToAction("Login", "Auth", new { area = "Renter" });

            // Basic validation
            if (desiredDurationMonths <= 0 || desiredDurationMonths > 24)
            {
                TempData["ErrorMessage"] = "Thời hạn thuê không hợp lệ.";
                return RedirectToAction(nameof(Detail), new { id = roomId });
            }

            var exists = await _db.BookingRequests.AnyAsync(b => b.IdRenter == personId && b.IdRoom == roomId && b.Status == "Pending");
            if (!exists)
            {
                var br = new BookingRequest
                {
                    Id = Guid.NewGuid(),
                    IdRenter = personId.Value,
                    IdRoom = roomId,
                    DesiredStartDate = desiredStartDate,
                    DesiredDurationMonths = desiredDurationMonths,
                    Note = note ?? string.Empty,
                    Status = "Pending",
                    DateCreated = DateTime.UtcNow
                };
                _db.BookingRequests.Add(br);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã gửi yêu cầu đặt phòng. Vui lòng chờ chủ trọ xác nhận.";
            }
            else
            {
                TempData["InfoMessage"] = "Bạn đã gửi yêu cầu đang chờ xác nhận cho phòng này.";
            }
            return RedirectToAction(nameof(Detail), new { id = roomId });
        }
    }
}
