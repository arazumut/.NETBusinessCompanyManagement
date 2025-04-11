using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace CompanyManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments.ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Employees)
                    .ThenInclude(e => e.Position)
                .Include(d => d.Positions)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Department department)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{department.Name} departmanı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Departman eklenirken bir hata oluştu: " + ex.Message);
                // Hata ayrıntılarını loglayalım
                LogError(ex, "Departman ekleme hatası");
            }
            
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{department.Name} departmanı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DepartmentExists(department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Departman güncellenirken bir hata oluştu: " + ex.Message);
                        // Hata ayrıntılarını loglayalım
                        LogError(ex, "Departman güncelleme hatası");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Departman güncellenirken bir hata oluştu: " + ex.Message);
                    // Hata ayrıntılarını loglayalım
                    LogError(ex, "Departman güncelleme hatası");
                }
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Employees)
                    .Include(d => d.Positions)
                    .FirstOrDefaultAsync(d => d.Id == id);
                
                if (department != null)
                {
                    // İlişkili kayıtları kontrol et
                    if ((department.Employees != null && department.Employees.Any()) || 
                        (department.Positions != null && department.Positions.Any()))
                    {
                        TempData["ErrorMessage"] = "Bu departmana bağlı çalışanlar veya pozisyonlar bulunduğu için silinemez.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{department.Name} departmanı başarıyla silindi.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Departman silinirken bir hata oluştu: " + ex.Message;
                // Hata ayrıntılarını loglayalım
                LogError(ex, "Departman silme hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
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
            
            // Prodüksiyonda gerçek loglama mekanizması kullanılabilir
            // _logger.LogError(ex, message);
        }
    }
} 