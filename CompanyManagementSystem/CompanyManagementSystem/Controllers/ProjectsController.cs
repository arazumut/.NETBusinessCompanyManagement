using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

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
            return View(await _context.Projects.ToListAsync());
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
            ViewBag.Employees = new MultiSelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Status")] Project project, int[] selectedEmployees)
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
                
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Employees = new MultiSelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", selectedEmployees);
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
            
            var selectedEmployees = project.Employees?.Select(e => e.Id).ToArray() ?? Array.Empty<int>();
            ViewBag.Employees = new MultiSelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", selectedEmployees);
            
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Status")] Project project, int[] selectedEmployees)
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
                    
                    // Çalışanları güncelle
                    existingProject.Employees.Clear();
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            
            ViewBag.Employees = new MultiSelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", selectedEmployees);
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
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (project == null)
            {
                return NotFound();
            }

            // İlişkili görevler var mı kontrol et
            var hasTasks = await _context.WorkTasks.AnyAsync(t => t.ProjectId == id);
            ViewBag.HasTasks = hasTasks;

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project != null)
            {
                // İlişkili görevler var mı kontrol et
                var hasTasks = await _context.WorkTasks.AnyAsync(t => t.ProjectId == id);
                
                if (hasTasks)
                {
                    ModelState.AddModelError(string.Empty, "Bu projeye atanmış görevler olduğu için silinemez.");
                    return View(project);
                }
                
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 