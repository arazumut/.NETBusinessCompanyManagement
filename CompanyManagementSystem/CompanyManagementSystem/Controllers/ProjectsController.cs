using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Collections.Generic;

namespace CompanyManagementSystem.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Employees)
                .Include(p => p.Tasks)
                .ToListAsync();
                
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Employees)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            var customers = _context.Customers.Where(c => c.IsActive).ToList();
            
            ViewBag.Employees = new MultiSelectList(employees, "Id", "FullName");
            ViewBag.Customers = new SelectList(customers, "Id", "Name");
            
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Status,CustomerId")] Project project, int[] selectedEmployees)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    
                    // Seçilen çalışanları projeye ekle
                    if (selectedEmployees != null && selectedEmployees.Length > 0)
                    {
                        project.Employees = new List<Employee>();
                        foreach (var employeeId in selectedEmployees)
                        {
                            var employee = await _context.Employees.FindAsync(employeeId);
                            if (employee != null)
                            {
                                project.Employees.Add(employee);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    
                    TempData["SuccessMessage"] = $"{project.Name} projesi başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Proje eklenirken bir hata oluştu: " + ex.Message);
                LogError(ex, "Proje ekleme hatası");
            }
            
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            var customers = _context.Customers.Where(c => c.IsActive).ToList();
            
            ViewBag.Employees = new MultiSelectList(employees, "Id", "FullName", selectedEmployees);
            ViewBag.Customers = new SelectList(customers, "Id", "Name", project.CustomerId);
            
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (project == null)
            {
                return NotFound();
            }
            
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            var customers = _context.Customers.Where(c => c.IsActive).ToList();
            var selectedEmployees = project.Employees?.Select(e => e.Id).ToArray() ?? Array.Empty<int>();
            
            ViewBag.Employees = new MultiSelectList(employees, "Id", "FullName", selectedEmployees);
            ViewBag.Customers = new SelectList(customers, "Id", "Name", project.CustomerId);
            
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Status,CustomerId")] Project project, int[] selectedEmployees)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut projeyi ve ilişkili çalışanları getir
                    var existingProject = await _context.Projects
                        .Include(p => p.Employees)
                        .FirstOrDefaultAsync(p => p.Id == id);
                        
                    if (existingProject == null)
                    {
                        return NotFound();
                    }
                    
                    // Proje bilgilerini güncelle
                    existingProject.Name = project.Name;
                    existingProject.Description = project.Description;
                    existingProject.StartDate = project.StartDate;
                    existingProject.EndDate = project.EndDate;
                    existingProject.Status = project.Status;
                    existingProject.CustomerId = project.CustomerId;
                    
                    // Çalışanları güncelle
                    if (existingProject.Employees == null)
                    {
                        existingProject.Employees = new List<Employee>();
                    }
                    else
                    {
                        existingProject.Employees.Clear();
                    }
                    
                    if (selectedEmployees != null && selectedEmployees.Length > 0)
                    {
                        foreach (var employeeId in selectedEmployees)
                        {
                            var employee = await _context.Employees.FindAsync(employeeId);
                            if (employee != null)
                            {
                                existingProject.Employees.Add(employee);
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{project.Name} projesi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Proje güncellenirken eşzamanlılık hatası oluştu: " + ex.Message);
                        LogError(ex, "Proje güncelleme eşzamanlılık hatası");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Proje güncellenirken bir hata oluştu: " + ex.Message);
                    LogError(ex, "Proje güncelleme hatası");
                }
            }
            
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            var customers = _context.Customers.Where(c => c.IsActive).ToList();
            
            ViewBag.Employees = new MultiSelectList(employees, "Id", "FullName", selectedEmployees);
            ViewBag.Customers = new SelectList(customers, "Id", "Name", project.CustomerId);
            
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (project == null)
            {
                return NotFound();
            }

            // İlişkili görevler var mı kontrol et
            var hasTasks = project.Tasks != null && project.Tasks.Any();
            ViewBag.HasTasks = hasTasks;
            
            // İlişkili çalışanlar var mı kontrol et
            var hasEmployees = project.Employees != null && project.Employees.Any();
            ViewBag.HasEmployees = hasEmployees;

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (project == null)
                {
                    return NotFound();
                }
                
                // İlişkili görevler var mı kontrol et
                if (project.Tasks != null && project.Tasks.Any())
                {
                    TempData["ErrorMessage"] = "Bu projeye atanmış görevler olduğu için silinemez.";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{project.Name} projesi başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Proje silinirken bir hata oluştu: " + ex.Message;
                LogError(ex, "Proje silme hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
        
        protected void LogError(Exception ex, string message)
        {
            // Hata mesajını debug penceresine yazdır
            Debug.WriteLine($"HATA: {message}");
            Debug.WriteLine($"Hata Mesajı: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            // İç içe hata varsa onu da logla
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"İç Hata: {ex.InnerException.Message}");
                Debug.WriteLine($"İç Hata Stack Trace: {ex.InnerException.StackTrace}");
            }
        }
    }
} 