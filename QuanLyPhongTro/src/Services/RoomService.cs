using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.src.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.src.Services
{
    public class RoomService
    {
        private readonly AppDbContext _context;
        public RoomService(AppDbContext context) { _context = context; }

        public async Task<List<RoomViewModel>> GetAvailableRoomsAsync(string keyword = "", decimal? minPrice = null, decimal? maxPrice = null, decimal? minArea = null, decimal? maxArea = null)
        {
            var query = BaseQuery(keyword, minPrice, maxPrice, minArea, maxArea);
            var rooms = await query.ToListAsync();
            return MapRooms(rooms);
        }

        public async Task<List<RoomViewModel>> GetAvailableRoomsPageAsync(int skip, int take, string keyword = "", decimal? minPrice = null, decimal? maxPrice = null, decimal? minArea = null, decimal? maxArea = null)
        {
            var query = BaseQuery(keyword, minPrice, maxPrice, minArea, maxArea)
                .OrderBy(r => r.Name)
                .Skip(skip)
                .Take(take);
            var rooms = await query.ToListAsync();
            return MapRooms(rooms);
        }

        public async Task<RoomViewModel?> GetFeaturedRoomAsync()
        {
            var r = await _context.Rooms
               .Include(r => r.RoomImages)
               .Include(r => r.IdOwnerNavigation).ThenInclude(o => o.IdDetailNavigation)
               .Where(r => r.Status == "Trống" && !r.Name.StartsWith("Phòng trọ tiện nghi"))
               .OrderByDescending(r => r.Id)
               .FirstOrDefaultAsync();
            return r == null ? null : MapRoom(r);
        }

        public async Task<int> GetAvailableRoomsCountAsync(string keyword = "", decimal? minPrice = null, decimal? maxPrice = null, decimal? minArea = null, decimal? maxArea = null)
        {
            var query = BaseQuery(keyword, minPrice, maxPrice, minArea, maxArea);
            return await query.CountAsync();
        }

        public async Task<decimal> GetMaxAvailableRoomPriceAsync()
        {
            var maxList = await _context.Rooms
                .Where(r => r.Status == "Trống" && r.Price != null)
                .Select(r => r.Price)
                .ToListAsync();
            return maxList.Any() ? maxList.Max()!.Value : 0m;
        }

        private IQueryable<Room> BaseQuery(string keyword, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea)
        {
            var query = _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.IdOwnerNavigation).ThenInclude(o => o.IdDetailNavigation)
                .Where(r => r.Status == "Trống");

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r =>
                    r.Name.Contains(keyword) ||
                    (r.Address != null && r.Address.Contains(keyword)) ||
                    (r.Description != null && r.Description.Contains(keyword)) ||
                    (r.IdOwnerNavigation != null && (
                        r.IdOwnerNavigation.Username.Contains(keyword) ||
                        (r.IdOwnerNavigation.IdDetailNavigation != null && r.IdOwnerNavigation.IdDetailNavigation.Name != null && r.IdOwnerNavigation.IdDetailNavigation.Name.Contains(keyword))
                    )));
            }
            if (minPrice.HasValue) query = query.Where(r => r.Price >= minPrice);
            if (maxPrice.HasValue) query = query.Where(r => r.Price <= maxPrice);
            if (minArea.HasValue) query = query.Where(r => r.Area >= minArea);
            if (maxArea.HasValue) query = query.Where(r => r.Area <= maxArea);
            return query;
        }

        private List<RoomViewModel> MapRooms(IEnumerable<Room> rooms) => rooms.Select(MapRoom).ToList();
        private RoomViewModel MapRoom(Room r) => new RoomViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Address = r.Address ?? string.Empty,
            Price = r.Price ?? 0,
            Area = (double)(r.Area ?? 0),
            Status = r.Status ?? string.Empty,
            Description = r.Description ?? string.Empty,
            MainImage = r.RoomImages.OrderBy(i => i.ImageUrl).FirstOrDefault()?.ImageUrl ?? "/images/no-image.svg",
            AllImages = r.RoomImages.Select(i => i.ImageUrl!).ToList(),
            OwnerName = r.IdOwnerNavigation?.IdDetailNavigation?.Name ?? r.IdOwnerNavigation?.Username ?? "Chủ trọ"
        };
    }
}