using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CompanyManagementSystem.ViewModels;

namespace CompanyManagementSystem.Controllers
{
    [Authorize]
    public class LeavesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LeavesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leaves
        public async Task<IActionResult> Index()
        {
            // Yöneticiler tüm izinleri görür
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                var allLeaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .OrderByDescending(l => l.RequestDate)
                    .ToListAsync();
                    
                return View(allLeaves);
            }
            
            // Çalışanlar sadece kendi izinlerini görür
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
                
            if (employee == null)
            {
                return View(new List<Leave>());
            }
            
            var myLeaves = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == employee.Id)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();
                
            return View(myLeaves);
        }

        // GET: Leaves/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (leave == null)
            {
                return NotFound();
            }
            
            // Sadece yöneticiler veya izin sahibi görüntüleyebilir
            var user = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
                
            if (!(User.IsInRole("Admin") || User.IsInRole("Manager") || 
                (employee != null && employee.Id == leave.EmployeeId)))
            {
                return Forbid();
            }

            return View(leave);
        }

        // GET: Leaves/Create
        public async Task<IActionResult> Create()
        {
            // Yöneticiler herhangi bir çalışan için izin oluşturabilir
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                ViewBag.EmployeeId = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName");
                return View();
            }
            
            // Çalışanlar sadece kendileri için izin oluşturabilir
            var user = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
                
            if (employee == null)
            {
                return NotFound("Personel kaydınız bulunamadı. Lütfen yöneticinize başvurun.");
            }
            
            ViewBag.EmployeeId = new SelectList(new[] { employee }, "Id", "FullName");
            
            var model = new Leave
            {
                EmployeeId = employee.Id,
                RequestDate = DateTime.Now,
                Status = LeaveStatus.Pending
            };
            
            return View(model);
        }

        // POST: Leaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,LeaveType,StartDate,EndDate,Description")] Leave leave)
        {
            // Yönetici olmayan kullanıcılar sadece kendi adlarına izin talep edebilir
            if (!(User.IsInRole("Admin") || User.IsInRole("Manager")))
            {
                var user = await _userManager.GetUserAsync(User);
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);
                    
                if (employee == null || employee.Id != leave.EmployeeId)
                {
                    return Forbid();
                }
            }
            
            if (ModelState.IsValid)
            {
                // İzin başlangıç tarihi bugünden önce olamaz
                if (leave.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("StartDate", "İzin başlangıç tarihi bugünden önce olamaz.");
                    PrepareEmployeeList(leave.EmployeeId);
                    return View(leave);
                }
                
                // İzin bitiş tarihi başlangıç tarihinden önce olamaz
                if (leave.EndDate < leave.StartDate)
                {
                    ModelState.AddModelError("EndDate", "İzin bitiş tarihi başlangıç tarihinden önce olamaz.");
                    PrepareEmployeeList(leave.EmployeeId);
                    return View(leave);
                }
                
                // Talep bilgileri
                leave.RequestDate = DateTime.Now;
                leave.Status = LeaveStatus.Pending;
                
                _context.Add(leave);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            PrepareEmployeeList(leave.EmployeeId);
            return View(leave);
        }

        // GET: Leaves/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }
            
            // Onaylanmış veya reddedilmiş izinler düzenlenemez
            if (leave.Status != LeaveStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id = leave.Id });
            }
            
            ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName", leave.EmployeeId);
            return View(leave);
        }

        // POST: Leaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,LeaveType,StartDate,EndDate,Description,Status,RequestDate")] Leave leave)
        {
            if (id != leave.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leave);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveExists(leave.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.EmployeeId = new SelectList(_context.Employees, "Id", "FullName", leave.EmployeeId);
            return View(leave);
        }

        // GET: Leaves/Approve/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (leave == null)
            {
                return NotFound();
            }
            
            // Sadece bekleyen izinler onaylanabilir
            if (leave.Status != LeaveStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id = leave.Id });
            }

            return View(leave);
        }

        // POST: Leaves/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ApproveConfirmed(int id, string actionReason)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }
            
            // Sadece bekleyen izinler onaylanabilir
            if (leave.Status != LeaveStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id = leave.Id });
            }
            
            // İzni onayla
            leave.Status = LeaveStatus.Approved;
            leave.ActionDate = DateTime.Now;
            leave.ActionReason = actionReason;
            leave.ActionBy = User.Identity.Name;
            
            _context.Update(leave);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Leaves/Reject/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (leave == null)
            {
                return NotFound();
            }
            
            // Sadece bekleyen izinler reddedilebilir
            if (leave.Status != LeaveStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id = leave.Id });
            }

            return View(leave);
        }

        // POST: Leaves/Reject/5
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RejectConfirmed(int id, string actionReason)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }
            
            // Sadece bekleyen izinler reddedilebilir
            if (leave.Status != LeaveStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id = leave.Id });
            }
            
            // Reddedilme nedeni boş olamaz
            if (string.IsNullOrWhiteSpace(actionReason))
            {
                ModelState.AddModelError("", "Reddetme nedeni belirtilmelidir.");
                var leaveWithEmployee = await _context.Leaves
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(l => l.Id == id);
                return View(leaveWithEmployee);
            }
            
            // İzni reddet
            leave.Status = LeaveStatus.Rejected;
            leave.ActionDate = DateTime.Now;
            leave.ActionReason = actionReason;
            leave.ActionBy = User.Identity.Name;
            
            _context.Update(leave);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Leaves/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (leave == null)
            {
                return NotFound();
            }

            return View(leave);
        }

        // POST: Leaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave != null)
            {
                _context.Leaves.Remove(leave);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveExists(int id)
        {
            return _context.Leaves.Any(e => e.Id == id);
        }
        
        private void PrepareEmployeeList(int? selectedId = null)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                ViewBag.EmployeeId = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", selectedId);
            }
            else
            {
                var user = _userManager.GetUserAsync(User).Result;
                var employee = _context.Employees.FirstOrDefault(e => e.UserId == user.Id);
                
                if (employee != null)
                {
                    ViewBag.EmployeeId = new SelectList(new[] { employee }, "Id", "FullName", employee.Id);
                }
            }
        }
    }
} 