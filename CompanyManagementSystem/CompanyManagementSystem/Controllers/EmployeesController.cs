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
    [Authorize(Roles = "Admin,Manager")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.User)
                .ToListAsync();
                
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.User)
                .Include(e => e.Leaves)
                .Include(e => e.Projects)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı hesabını oluştur
                var user = new IdentityUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    EmailConfirmed = true 
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Personel rolünü ata
                    await _userManager.AddToRoleAsync(user, "Employee");
                    
                    // Personel kaydını oluştur
                    var employee = new Employee
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        IdentityNumber = model.IdentityNumber,
                        Phone = model.Phone,
                        Address = model.Address,
                        DepartmentId = model.DepartmentId,
                        PositionId = model.PositionId,
                        StartDate = model.StartDate,
                        Salary = model.Salary,
                        IsActive = true,
                        UserId = user.Id
                    };
                    
                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", model.PositionId);
            return View(model);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,IdentityNumber,Email,Phone,Address,DepartmentId,PositionId,StartDate,Salary,IsActive,UserId")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı e-postasını güncelle
                    var user = await _userManager.FindByIdAsync(employee.UserId);
                    if (user != null && user.Email != employee.Email)
                    {
                        user.Email = employee.Email;
                        user.UserName = employee.Email;
                        user.NormalizedEmail = employee.Email.ToUpper();
                        user.NormalizedUserName = employee.Email.ToUpper();
                        
                        await _userManager.UpdateAsync(user);
                    }
                    
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (employee != null)
            {
                // İlişkili kullanıcıyı sil
                if (employee.User != null)
                {
                    await _userManager.DeleteAsync(employee.User);
                }
                
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 