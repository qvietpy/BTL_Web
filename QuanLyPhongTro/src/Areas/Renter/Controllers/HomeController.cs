using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.src.Services;
using System.Threading.Tasks;
using QuanLyPhongTro.Data;
using System.Linq;

namespace QuanLyPhongTro.Areas.Renter.Controllers
{
    [Area("Renter")]
    public class HomeController : Controller
    {
        private readonly RoomService _roomService;
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private const int PageSize = 8;

        public HomeController(RoomService roomService, ILogger<HomeController> logger, AppDbContext context)
        {
            _roomService = roomService;
            _logger = logger;
            _context = context; 
        }

        public async Task<IActionResult> Index(string keyword, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea)
        {
            var featuredRoom = await _roomService.GetFeaturedRoomAsync();
            var rooms = await _roomService.GetAvailableRoomsPageAsync(0, PageSize, keyword, minPrice, maxPrice, minArea, maxArea);
            var total = await _roomService.GetAvailableRoomsCountAsync(keyword, minPrice, maxPrice, minArea, maxArea);
            var maxRoomPrice = await _roomService.GetMaxAvailableRoomPriceAsync();
            ViewBag.FeaturedRoom = featuredRoom;
            ViewBag.TotalRooms = total;
            ViewBag.InitialLoaded = rooms.Count;
            ViewBag.MaxPrice = maxRoomPrice;
            ViewBag.Keyword = keyword;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPriceFilter = maxPrice;
            ViewBag.MinArea = minArea;
            ViewBag.MaxArea = maxArea;
            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea)
        {
            var filteredRooms = await _roomService.GetAvailableRoomsAsync(keyword, minPrice, maxPrice, minArea, maxArea);
            var maxRoomPrice = await _roomService.GetMaxAvailableRoomPriceAsync();
            ViewBag.SearchMode = true;
            ViewBag.MaxPrice = maxRoomPrice;
            ViewBag.Keyword = keyword;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPriceFilter = maxPrice;
            ViewBag.MinArea = minArea;
            ViewBag.MaxArea = maxArea;
            ViewBag.TotalRooms = filteredRooms.Count;
            ViewBag.EmptySearch = !filteredRooms.Any();
            return View("Index", filteredRooms);
        }

        [HttpGet]
        public async Task<IActionResult> Reset()
        {
            var maxRoomPrice = await _roomService.GetMaxAvailableRoomPriceAsync();
            return RedirectToAction(nameof(Index), new { minPrice = 0m, maxPrice = maxRoomPrice, keyword = string.Empty, minArea = (decimal?)null, maxArea = (decimal?)null });
        }

        [HttpGet]
        public async Task<IActionResult> LoadMore(int skip, string keyword, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea)
        {
            var rooms = await _roomService.GetAvailableRoomsPageAsync(skip, PageSize, keyword, minPrice, maxPrice, minArea, maxArea);
            return Json(new { items = rooms, count = rooms.Count });
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string keyword, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea)
        {
            var rooms = await _roomService.GetAvailableRoomsPageAsync(0, PageSize, keyword, minPrice, maxPrice, minArea, maxArea);
            var total = await _roomService.GetAvailableRoomsCountAsync(keyword, minPrice, maxPrice, minArea, maxArea);
            return Json(new { items = rooms, count = rooms.Count, total });
        }

        public IActionResult TrangChu()
        {
            var lstRoom = _context.Rooms.ToList();
            ViewData["lstRoom"] = lstRoom;
            return View();
        }
    }
}