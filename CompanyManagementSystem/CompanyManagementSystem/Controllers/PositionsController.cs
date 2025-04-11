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
    [Authorize(Roles = "Admin,Manager")]
    public class PositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Positions
        public async Task<IActionResult> Index()
        {
            var positions = await _context.Positions
                .Include(p => p.Department)
                .ToListAsync();
                
            return View(positions);
        }

        // GET: Positions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Department)
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }

        // GET: Positions/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Positions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,DepartmentId")] Position position)
        {
            if (ModelState.IsValid)
            {
                _context.Add(position);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", position.DepartmentId);
            return View(position);
        }

        // GET: Positions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions.FindAsync(id);
            
            if (position == null)
            {
                return NotFound();
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", position.DepartmentId);
            return View(position);
        }

        // POST: Positions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DepartmentId")] Position position)
        {
            if (id != position.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(position);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PositionExists(position.Id))
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
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", position.DepartmentId);
            return View(position);
        }

        // GET: Positions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var position = await _context.Positions
                .Include(p => p.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (position == null)
            {
                return NotFound();
            }

            // İlişkili personel var mı kontrol et
            var hasEmployees = await _context.Employees.AnyAsync(e => e.PositionId == id);
            ViewBag.HasEmployees = hasEmployees;

            return View(position);
        }

        // POST: Positions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            
            if (position != null)
            {
                // İlişkili personel var mı kontrol et
                var hasEmployees = await _context.Employees.AnyAsync(e => e.PositionId == id);
                
                if (hasEmployees)
                {
                    ModelState.AddModelError(string.Empty, "Bu pozisyona atanmış personeller olduğu için silinemez.");
                    ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", position.DepartmentId);
                    
                    var positionWithDepartment = await _context.Positions
                        .Include(p => p.Department)
                        .FirstOrDefaultAsync(m => m.Id == id);
                        
                    ViewBag.HasEmployees = true;
                    return View(positionWithDepartment);
                }
                
                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool PositionExists(int id)
        {
            return _context.Positions.Any(e => e.Id == id);
        }
    }
} 