using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuShop.Data;
using MenuShop.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MenuShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập và mật khẩu");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            // Update last login time
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("IsAdmin", user.IsAdmin.ToString()),
                new Claim("Avatar", user.Avatar ?? "")
            };

            // Add role claim if user is admin
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login");

            var vm = new UserProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile? avatarFile)
        {
            var username = User.Identity?.Name;
            Console.WriteLine("[DEBUG] User.Identity.Name: " + username);

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                // Thử lấy theo Id từ claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine("[DEBUG] Không tìm thấy user theo username. Thử theo Id: " + userIdClaim);
                if (int.TryParse(userIdClaim, out int userId))
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                }
            }
            if (user == null)
            {
                Console.WriteLine("[ERROR] Không tìm thấy user để cập nhật");
                TempData["Error"] = "Không tìm thấy tài khoản để cập nhật.";
                return RedirectToAction("Profile");
            }

            Console.WriteLine($"[BEFORE] FullName: {user!.FullName}, Email: {user!.Email}, Phone: {user!.Phone}, Avatar: {user!.Avatar}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[DEBUG] ModelState không hợp lệ:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"[MODEL ERROR] {key}: {error.ErrorMessage}");
                    }
                }
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                // Trả lại view với model (viewmodel)
                return View(model);
            }

            // Cập nhật thông tin cơ bản
            user!.FullName = model.FullName;
            user!.Email = model.Email;
            user!.Phone = model.Phone;

            // Xử lý upload avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        var oldAvatarPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars", user.Avatar);
                        if (System.IO.File.Exists(oldAvatarPath))
                        {
                            System.IO.File.Delete(oldAvatarPath);
                        }
                    }

                    var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    user.Avatar = fileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Upload avatar: " + ex);
                    TempData["Error"] = "Lỗi upload avatar.";
                }
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[AFTER] FullName: {user.FullName}, Email: {user.Email}, Phone: {user.Phone}, Avatar: {user.Avatar}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SAVE ERROR] " + ex);
                TempData["Error"] = "Lỗi lưu dữ liệu.";
                return View(model);
            }

            // Refresh claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("IsAdmin", user.IsAdmin.ToString()),
                new Claim("Avatar", user.Avatar ?? "")
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }
    }
} 