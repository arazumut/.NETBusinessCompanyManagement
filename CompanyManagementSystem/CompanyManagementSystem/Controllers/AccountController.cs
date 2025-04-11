using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using CompanyManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CompanyManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl ?? "/");
                }
                if (result.IsLockedOut)
                {
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Yeni kullanıcıya Employee rolü atama
                    await _userManager.AddToRoleAsync(user, "Employee");
                    
                    // Otomatik giriş yap
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl ?? "/");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
                
            if (employee == null)
            {
                // Kullanıcı var ama personel kaydı yoksa, sadece kullanıcı bilgilerini içeren profil göster
                var userProfile = new UserProfileViewModel
                {
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Roles = await _userManager.GetRolesAsync(user)
                };
                
                return View(userProfile);
            }
            
            // Çalışan bilgilerini içeren profil göster
            var employeeProfile = new EmployeeProfileViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = employee.FullName,
                Email = employee.Email,
                Phone = employee.Phone,
                Address = employee.Address,
                IdentityNumber = employee.IdentityNumber,
                Department = employee.Department?.Name ?? string.Empty,
                Position = employee.Position?.Name ?? string.Empty,
                StartDate = employee.StartDate,
                IsActive = employee.IsActive,
                Roles = await _userManager.GetRolesAsync(user)
            };
            
            return View(employeeProfile);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RolTesti()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            var allUsers = await _userManager.Users.ToListAsync();
            
            var viewModel = new Dictionary<string, List<string>>();
            
            foreach (var user in allUsers)
            {
                viewModel[user.Email ?? user.UserName ?? "Bilinmeyen Kullanıcı"] = 
                    (await _userManager.GetRolesAsync(user)).ToList();
            }
            
            ViewBag.AllRoles = allRoles.Select(r => r.Name).ToList();
            
            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KullaniciRolleri()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, List<string>>();
            
            foreach (var user in users)
            {
                userRoles[user.Id] = (await _userManager.GetRolesAsync(user)).ToList();
            }
            
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Users = users;
            ViewBag.UserRoles = userRoles;
            
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciRolDegistir([Bind("UserId,Role,IsChecked")] RolDegistirViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }
            
            // Role zaten sahipse ve kaldırılıyorsa
            if (await _userManager.IsInRoleAsync(user, model.Role) && !model.IsChecked)
            {
                // Son admin rolünü kaldırmaya çalışıyorsa engelle
                if (model.Role == "Admin")
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                    if (adminUsers.Count <= 1)
                    {
                        return Json(new { success = false, message = "Sistemde en az bir admin kalmalıdır." });
                    }
                }
                
                await _userManager.RemoveFromRoleAsync(user, model.Role);
                return Json(new { success = true, message = $"{user.Email} kullanıcısından {model.Role} rolü kaldırıldı." });
            }
            // Role sahip değilse ve ekleniyor
            else if (!await _userManager.IsInRoleAsync(user, model.Role) && model.IsChecked)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Json(new { success = true, message = $"{user.Email} kullanıcısına {model.Role} rolü eklendi." });
            }
            
            return Json(new { success = true, message = "İşlem gerçekleştirildi." });
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
} 