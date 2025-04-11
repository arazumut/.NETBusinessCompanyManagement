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
    public class WorkTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WorkTasks
        public async Task<IActionResult> Index()
        {
            var workTasks = await _context.WorkTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .ToListAsync();
                
            return View(workTasks);
        }

        // GET: WorkTasks/MyTasks
        public async Task<IActionResult> MyTasks()
        {
            // Kullanıcı ile ilişkili çalışanı bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }
            
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (employee == null)
            {
                return View(new List<WorkTask>());
            }
            
            // Çalışana atanmış görevleri getir
            var myTasks = await _context.WorkTasks
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == employee.Id)
                .ToListAsync();
                
            return View("Index", myTasks);
        }

        // GET: WorkTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workTask = await _context.WorkTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (workTask == null)
            {
                return NotFound();
            }

            return View(workTask);
        }

        // GET: WorkTasks/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["AssignedToId"] = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName");
            return View();
        }

        // POST: WorkTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Name,Description,ProjectId,AssignedToId,StartDate,DueDate,Status,Priority,CompletionPercentage")] WorkTask workTask)
        {
            if (ModelState.IsValid)
            {
                workTask.CreatedAt = DateTime.Now;
                _context.Add(workTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", workTask.ProjectId);
            ViewData["AssignedToId"] = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", workTask.AssignedToId);
            return View(workTask);
        }

        // GET: WorkTasks/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workTask = await _context.WorkTasks.FindAsync(id);
            if (workTask == null)
            {
                return NotFound();
            }
            
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", workTask.ProjectId);
            ViewData["AssignedToId"] = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", workTask.AssignedToId);
            return View(workTask);
        }

        // POST: WorkTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ProjectId,AssignedToId,StartDate,DueDate,Status,Priority,CompletionPercentage,CreatedAt")] WorkTask workTask)
        {
            if (id != workTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    workTask.UpdatedAt = DateTime.Now;
                    _context.Update(workTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkTaskExists(workTask.Id))
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
            
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", workTask.ProjectId);
            ViewData["AssignedToId"] = new SelectList(_context.Employees.Where(e => e.IsActive), "Id", "FullName", workTask.AssignedToId);
            return View(workTask);
        }

        // GET: WorkTasks/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workTask = await _context.WorkTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (workTask == null)
            {
                return NotFound();
            }

            return View(workTask);
        }

        // POST: WorkTasks/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, [Bind("Id,Status,CompletionPercentage")] WorkTask taskUpdate)
        {
            var workTask = await _context.WorkTasks.FindAsync(id);
            if (workTask == null)
            {
                return NotFound();
            }

            // Sadece durum ve tamamlanma yüzdesini güncelle
            workTask.Status = taskUpdate.Status;
            workTask.CompletionPercentage = taskUpdate.CompletionPercentage;
            workTask.UpdatedAt = DateTime.Now;
            
            // Eğer durum tamamlandı ise, tamamlanma yüzdesini 100 yap
            if (workTask.Status == WorkTaskStatus.Done)
            {
                workTask.CompletionPercentage = 100;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: WorkTasks/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workTask = await _context.WorkTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (workTask == null)
            {
                return NotFound();
            }

            return View(workTask);
        }

        // POST: WorkTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workTask = await _context.WorkTasks.FindAsync(id);
            if (workTask != null)
            {
                _context.WorkTasks.Remove(workTask);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool WorkTaskExists(int id)
        {
            return _context.WorkTasks.Any(e => e.Id == id);
        }
    }
} 