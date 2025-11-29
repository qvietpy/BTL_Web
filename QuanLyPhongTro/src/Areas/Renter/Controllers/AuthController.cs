using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.src.Models.ViewModels; // for LoginViewModel and RegisterViewModel
using QuanLyPhongTro.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using QuanLyPhongTro.src.Models.ViewModels; // added for RoomListViewModel

namespace QuanLyPhongTro.src.Areas.Renter.Controllers
{
    [Area("Renter")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        // GET: Auth/Login
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển về trang chủ theo vai trò
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                if (string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Dashboard", new { area = "Owner" });
                // renter: show landing view directly
                var rooms = await _context.Rooms
                    .Include(r=>r.RoomImages)
                    .Include(r=>r.IdOwnerNavigation).ThenInclude(o=>o.IdDetailNavigation)
                    .OrderByDescending(r=>r.Id)
                    .Take(9)
                    .ToListAsync();
                var vm = rooms.Select(r=> new RoomListViewModel{
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Address = r.Address ?? "",
                    Area = r.Area ?? 0,
                    Price = r.Price ?? 0,
                    Status = r.Status ?? "",
                    Description = r.Description ?? "",
                    MaxOccupants = 0,
                    OwnerName = r.IdOwnerNavigation?.IdDetailNavigation?.Name ?? r.IdOwnerNavigation?.Username ?? "",
                    OwnerPhone = r.IdOwnerNavigation?.IdDetailNavigation?.Phone ?? "",
                    ImageUrls = r.RoomImages.Select(i=> i.ImageUrl ?? "").ToList()
                }).ToList();
                ViewBag.FullName = User.Identity?.Name;
                return View("/src/Views/Renter/Index.cshtml", vm);
            }
            return View(new LoginViewModel());
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _context.People
                .Include(p => p.IdDetailNavigation)
                .FirstOrDefaultAsync(p => p.Username == model.Username && p.Password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View(model);
            }
            // Claims + cookie sign-in
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.IdDetailNavigation?.Name ?? user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            var role = (user.Role ?? string.Empty).Trim();
            if (string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Dashboard", new { area = "Owner" });

            // renter: build featured rooms model and render non-area view
            var rooms = await _context.Rooms
                .Include(r=>r.RoomImages)
                .Include(r=>r.IdOwnerNavigation).ThenInclude(o=>o.IdDetailNavigation)
                .OrderByDescending(r=>r.Id)
                .Take(9)
                .ToListAsync();
            var vm = rooms.Select(r=> new RoomListViewModel{
                Id = r.Id,
                Name = r.Name ?? "",
                Address = r.Address ?? "",
                Area = r.Area ?? 0,
                Price = r.Price ?? 0,
                Status = r.Status ?? "",
                Description = r.Description ?? "",
                MaxOccupants = 0,
                OwnerName = r.IdOwnerNavigation?.IdDetailNavigation?.Name ?? r.IdOwnerNavigation?.Username ?? "",
                OwnerPhone = r.IdOwnerNavigation?.IdDetailNavigation?.Phone ?? "",
                ImageUrls = r.RoomImages.Select(i=> i.ImageUrl ?? "").ToList()
            }).ToList();
            ViewBag.FullName = user.IdDetailNavigation?.Name ?? user.Username;
            return View("/src/Views/Renter/Index.cshtml", vm);
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var existingUser = await _context.People.FirstOrDefaultAsync(p => p.Username == model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại!");
                return View(model);
            }
            var existingEmail = await _context.PersonDetails.FirstOrDefaultAsync(pd => pd.Gmail == model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng!");
                return View(model);
            }
            var detail = new PersonDetail
            {
                Id = Guid.NewGuid(),
                Name = model.FullName,
                Gmail = model.Email,
                Phone = model.Phone,
                Cccd = model.CCCD,
                Gender = model.Gender
            };
            _context.PersonDetails.Add(detail);
            await _context.SaveChangesAsync();
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                Password = model.Password, // TODO: hash
                Role = model.Role,
                IdDetail = detail.Id
            };
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Login));
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}