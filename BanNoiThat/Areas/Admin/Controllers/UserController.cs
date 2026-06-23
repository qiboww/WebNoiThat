using BanNoiThat.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BanNoiThat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/User")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ==========================================
        // VIEW ROUTES (Giao diện HTML)
        // ==========================================

        // GET: /Admin/User
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/User/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Admin/User/Details/{id}
        [HttpGet("Details/{id}")]
        public IActionResult Details(string id)
        {
            ViewBag.UserId = id;
            return View();
        }

        // GET: /Admin/User/Edit/{id}
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(string id)
        {
            ViewBag.UserId = id;
            return View();
        }

        // ==========================================
        // REST API ENDPOINTS (JSON Data)
        // ==========================================

        // GET: /api/users
        [HttpGet("/api/users")]
        public async Task<IActionResult> GetUsersApi()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
                userList.Add(new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    roles = roles,
                    isLockedOut = isLockedOut,
                    lockoutEnd = user.LockoutEnd
                });
            }

            return Ok(userList);
        }

        // GET: /api/users/{id}
        [HttpGet("/api/users/{id}")]
        public async Task<IActionResult> GetUserApi(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng với ID: {id}." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                roles = roles,
                isLockedOut = isLockedOut,
                lockoutEnd = user.LockoutEnd
            });
        }

        // POST: /api/users
        [HttpPost("/api/users")]
        public async Task<IActionResult> CreateUserApi([FromForm] UserCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email này đã được sử dụng bởi một tài khoản khác." });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email, // Thường dùng email làm UserName trong ASP.NET Core Identity
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            if (model.Roles != null && model.Roles.Any())
            {
                foreach (var role in model.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                await _userManager.AddToRolesAsync(user, model.Roles);
            }

            return Ok(new { success = true, userId = user.Id });
        }

        // PUT: /api/users/{id}
        [HttpPut("/api/users/{id}")]
        public async Task<IActionResult> UpdateUserApi(string id, [FromForm] UserUpdateModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            user.Email = model.Email;
            user.UserName = model.Email; // Cập nhật UserName theo Email mới
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            // Xử lý đổi mật khẩu nếu được nhập
            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!passResult.Succeeded)
                {
                    return BadRequest(new { message = string.Join(" ", passResult.Errors.Select(e => e.Description)) });
                }
            }

            // Cập nhật Roles
            if (model.Roles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                foreach (var role in model.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                await _userManager.AddToRolesAsync(user, model.Roles);
            }

            return Ok(new { success = true });
        }

        // DELETE: /api/users/{id}
        [HttpDelete("/api/users/{id}")]
        public async Task<IActionResult> DeleteUserApi(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Ngăn chặn việc tự xóa tài khoản của bản thân
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser != null && loggedInUser.Id == user.Id)
            {
                return BadRequest(new { message = "Bạn không thể tự xóa tài khoản của chính mình." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { success = true });
        }

        // POST: /api/users/{id}/toggle-lock
        [HttpPost("/api/users/{id}/toggle-lock")]
        public async Task<IActionResult> ToggleLockUserApi(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Ngăn chặn việc tự khóa tài khoản của bản thân
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser != null && loggedInUser.Id == user.Id)
            {
                return BadRequest(new { message = "Bạn không thể tự khóa tài khoản của chính mình." });
            }

            bool isLocked;
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Mở khóa
                user.LockoutEnd = null;
                isLocked = false;
            }
            else
            {
                // Khóa trong 100 năm
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                isLocked = true;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { success = true, isLocked = isLocked });
        }

        // GET: /api/roles
        [HttpGet("/api/roles")]
        public async Task<IActionResult> GetRolesApi()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            if (!roles.Contains("Admin")) roles.Add("Admin");
            if (!roles.Contains("User")) roles.Add("User");
            return Ok(roles);
        }
    }

    public class UserCreateModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserUpdateModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Password { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
