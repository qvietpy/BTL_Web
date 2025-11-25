using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.ViewModels; // corrected namespace
using QuanLyPhongTro.Models;
using System;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home", new { area = "Renter" });
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
                ModelState.AddModelError(string.Empty, "Tên ??ng nh?p ho?c m?t kh?u không ?úng!");
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
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Owner" });
            }
            // Renter (default). If renter already has an active/approved contract, go to Contracts/My
            var hasContract = await _context.Contracts.AnyAsync(c => c.IdRenter == user.Id && (c.Status == "Active" || c.Status == "Approved"));
            if (hasContract)
                return RedirectToAction("My", "Contracts", new { area = "Renter" });
            return RedirectToAction("Index", "Home", new { area = "Renter" });
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
                ModelState.AddModelError(nameof(model.Username), "Tên ??ng nh?p ?ã t?n t?i!");
                return View(model);
            }
            var existingEmail = await _context.PersonDetails.FirstOrDefaultAsync(pd => pd.Gmail == model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email ?ã ???c s? d?ng!");
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
