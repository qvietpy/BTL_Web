using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace QuanLyPhongTro.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) { _context = context; }

        private Guid? CurrentOwnerId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = CurrentOwnerId();
            var roomsQuery = _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.Contracts)
                    .ThenInclude(c => c.IdRenterNavigation)
                        .ThenInclude(p => p.IdDetailNavigation)
                .Where(r => ownerId == null || r.IdOwner == ownerId);

            var rooms = await roomsQuery.ToListAsync();

            int totalRooms = rooms.Count;
            int rentedRooms = rooms.Count(r => r.Status == "Rented" || r.Contracts.Any(c => c.Status == "Active"));
            int emptyRooms = rooms.Count(r => r.Status == "Empty" || (string.IsNullOrEmpty(r.Status)));
            int underMaintenance = rooms.Count(r => r.Status == "Maintenance");
            int activeContracts = await _context.Contracts.CountAsync(c => (ownerId==null || c.IdRoomNavigation!.IdOwner==ownerId) && c.Status == "Active");
            int pendingRequests = await _context.BookingRequests.CountAsync(b => (ownerId==null || b.Room.IdOwner==ownerId) && b.Status == "Pending");
            decimal monthlyRevenue = rooms.Where(r => r.Contracts.Any(c => c.Status == "Active"))
                                          .Sum(r => r.Price ?? 0);

            ViewBag.TotalRooms = totalRooms;
            ViewBag.RentedRooms = rentedRooms;
            ViewBag.EmptyRooms = emptyRooms;
            ViewBag.UnderMaintenance = underMaintenance;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.Rooms = rooms;
            return View();
        }
    }
}