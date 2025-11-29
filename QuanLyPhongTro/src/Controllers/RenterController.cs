using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.src.Models.ViewModels;

namespace QuanLyPhongTro.Controllers
{
    public class RenterController : Controller
    {
        private readonly AppDbContext _context;

        public RenterController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra đăng nhập
        private bool CheckLogin()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(userId) && role == "Renter";
        }

        // Lấy userId hiện tại
        private Guid GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetString("UserId");
            return Guid.Parse(userId!);
        }

        // GET: Renter/Index - Trang chủ Renter
        public async Task<IActionResult> Index()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Lấy danh sách phòng trống
            var rooms = await _context.Rooms
                .Include(r => r.IdOwnerNavigation)
                    .ThenInclude(o => o!.IdDetailNavigation)
                .Include(r => r.RoomImages)
                .Where(r => r.Status == "Trống")
                .OrderByDescending(r => r.Price)
                .Take(6)
                .Select(r => new RoomListViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address ?? "",
                    Area = r.Area ?? 0,
                    Price = r.Price ?? 0,
                    Status = r.Status ?? "",
                    Description = r.Description ?? "",
                    MaxOccupants = r.MaxOccupants,
                    OwnerName = r.IdOwnerNavigation!.IdDetailNavigation!.Name ?? "",
                    OwnerPhone = r.IdOwnerNavigation.IdDetailNavigation.Phone ?? "",
                    ImageUrls = r.RoomImages.Select(img => img.ImageUrl ?? "").ToList()
                })
                .ToListAsync();

            return View(rooms);
        }

        // GET: Renter/Rooms - Danh sách tất cả phòng với phân trang
        public async Task<IActionResult> Rooms(string? keyword, string? address, decimal? minPrice,
            decimal? maxPrice, decimal? minArea, decimal? maxArea, int? maxOccupants, int page = 1)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            int pageSize = 9; // 9 phòng mỗi trang

            var query = _context.Rooms
                .Include(r => r.IdOwnerNavigation)
                    .ThenInclude(o => o!.IdDetailNavigation)
                .Include(r => r.RoomImages)
                .Where(r => r.Status == "Trống")
                .AsQueryable();

            // Áp dụng filter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r => r.Name.Contains(keyword) ||
                                       (r.Description != null && r.Description.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                query = query.Where(r => r.Address != null && r.Address.Contains(address));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(r => r.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.Price <= maxPrice.Value);
            }

            if (minArea.HasValue)
            {
                query = query.Where(r => r.Area >= minArea.Value);
            }

            if (maxArea.HasValue)
            {
                query = query.Where(r => r.Area <= maxArea.Value);
            }

            if (maxOccupants.HasValue)
            {
                query = query.Where(r => r.MaxOccupants >= maxOccupants.Value);
            }

            // Tính tổng số phòng
            var totalRooms = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);

            // Lấy dữ liệu phân trang
            var rooms = await query
                .OrderByDescending(r => r.Price)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomListViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address ?? "",
                    Area = r.Area ?? 0,
                    Price = r.Price ?? 0,
                    Status = r.Status ?? "",
                    Description = r.Description ?? "",
                    MaxOccupants = r.MaxOccupants,
                    OwnerName = r.IdOwnerNavigation!.IdDetailNavigation!.Name ?? "",
                    OwnerPhone = r.IdOwnerNavigation!.IdDetailNavigation!.Phone ?? "",
                    ImageUrls = r.RoomImages.Select(img => img.ImageUrl ?? "").ToList()
                })
                .ToListAsync();

            var searchModel = new RoomSearchViewModel
            {
                Keyword = keyword,
                Address = address,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinArea = minArea,
                MaxArea = maxArea,
                MaxOccupants = maxOccupants,
                Results = rooms,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalRooms = totalRooms
            };

            return View(searchModel);
        }

        // GET: Renter/RoomDetail/id - Chi tiết phòng
        public async Task<IActionResult> RoomDetail(Guid id)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var room = await _context.Rooms
                .Include(r => r.IdOwnerNavigation)
                    .ThenInclude(o => o!.IdDetailNavigation)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng!";
                return RedirectToAction("Rooms");
            }

            // Lấy danh sách dịch vụ
            var services = await _context.Services
                .Take(10)
                .Select(s => new ServiceInfo
                {
                    Name = s.Name,
                    Unit = s.Unit ?? "",
                    PricePerUnit = s.PricePerUnit ?? 0
                })
                .ToListAsync();

            var viewModel = new RoomDetailViewModel
            {
                Id = room.Id,
                Name = room.Name,
                Address = room.Address ?? "",
                Area = room.Area ?? 0,
                Price = room.Price ?? 0,
                Status = room.Status ?? "",
                Description = room.Description ?? "",
                MaxOccupants = room.MaxOccupants,
                OwnerName = room.IdOwnerNavigation?.IdDetailNavigation?.Name ?? "",
                OwnerPhone = room.IdOwnerNavigation?.IdDetailNavigation?.Phone ?? "",
                OwnerEmail = room.IdOwnerNavigation?.IdDetailNavigation?.Gmail ?? "",
                ImageUrls = room.RoomImages.Select(img => img.ImageUrl ?? "").ToList(),
                Services = services
            };

            return View(viewModel);
        }

        // POST: Renter/RequestRental - Gửi yêu cầu thuê phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRental(RentalRequestViewModel model)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin!";
                return RedirectToAction("RoomDetail", new { id = model.RoomId });
            }

            try
            {
                // Kiểm tra phòng còn trống không
                var room = await _context.Rooms.FindAsync(model.RoomId);
                if (room == null || room.Status != "Trống")
                {
                    TempData["ErrorMessage"] = "Phòng không còn trống hoặc không tồn tại!";
                    return RedirectToAction("Rooms");
                }

                // Kiểm tra ngày hợp lệ
                if (model.EndDate <= model.StartDate)
                {
                    TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu!";
                    return RedirectToAction("RoomDetail", new { id = model.RoomId });
                }

                // Tạo contract mới
                var contract = new Contract
                {
                    Id = Guid.NewGuid(),
                    IdRoom = model.RoomId,
                    IdRenter = GetCurrentUserId(),
                    StartDate = DateOnly.FromDateTime(model.StartDate),
                    //StartDate = model.StartDate,
                    EndDate = DateOnly.FromDateTime(model.EndDate),
                    Deposit = model.Deposit,
                    Status = "Pending" // Đang chờ duyệt
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gửi yêu cầu thuê phòng thành công! Vui lòng chờ chủ nhà xác nhận.";
                return RedirectToAction("MyContracts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("RoomDetail", new { id = model.RoomId });
            }
        }

        // GET: Renter/MyContracts - Hợp đồng của tôi
        public async Task<IActionResult> MyContracts()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId();

            var contracts = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                    .ThenInclude(r => r!.IdOwnerNavigation)
                        .ThenInclude(o => o!.IdDetailNavigation)
                .Where(c => c.IdRenter == userId)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new RenterContractViewModel
                {
                    Id = c.Id,
                    RoomName = c.IdRoomNavigation!.Name,
                    RoomAddress = c.IdRoomNavigation.Address ?? "",
                    //StartDate = c.StartDate ?? DateTime.Now,
                    //EndDate = c.EndDate ?? DateTime.Now,
                    StartDate = (c.StartDate ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
                    EndDate = (c.EndDate ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
                    Deposit = c.Deposit ?? 0,
                    Status = c.Status ?? "",
                    OwnerName = c.IdRoomNavigation.IdOwnerNavigation!.IdDetailNavigation!.Name ?? "",
                    OwnerPhone = c.IdRoomNavigation.IdOwnerNavigation.IdDetailNavigation.Phone ?? ""
                })
                .ToListAsync();

            return View(contracts);
        }

        // GET: Renter/CreateReport - Trang tạo báo cáo
        public async Task<IActionResult> CreateReport()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            // Lấy danh sách phòng đang thuê
            var userId = GetCurrentUserId();
            var contracts = await _context.Contracts
                .Include(c => c.IdRoomNavigation)
                .Where(c => c.IdRenter == userId && c.Status == "Active")
                .ToListAsync();

            var rooms = contracts.Where(c => c.IdRoomNavigation != null).Select(c => c.IdRoomNavigation!).ToList();
            ViewBag.Rooms = rooms;

            return View(new ReportViewModel());
        }

        // POST: Renter/CreateReport - Tạo báo cáo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReport(ReportViewModel model)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var rooms = await _context.Contracts
                    .Include(c => c.IdRoomNavigation)
                    .Where(c => c.IdRenter == userId && c.Status == "Active")
                    .Select(c => c.IdRoomNavigation)
                    .ToListAsync();
                ViewBag.Rooms = rooms;
                return View(model);
            }

            try
            {
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    IdReporter = GetCurrentUserId(),
                    IdRoom = model.RoomId,
                    Title = model.Title,
                    Description = model.Description,
                    DateCreated = DateTime.Now,
                    Status = "Pending"
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gửi báo cáo thành công!";
                return RedirectToAction("MyReports");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return View(model);
            }
        }

        // GET: Renter/MyReports - Báo cáo của tôi
        public async Task<IActionResult> MyReports()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId();
            var reports = await _context.Reports
                .Include(r => r.IdRoomNavigation)
                .Where(r => r.IdReporter == userId)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();

            return View(reports);
        }
    }
}