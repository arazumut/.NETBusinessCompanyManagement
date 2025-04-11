using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Data;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CompanyManagementSystem.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ApplicationDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string searchString, string status, CustomerType? customerType)
        {
            var customersQuery = _context.Customers
                .Include(c => c.Projects)
                .AsQueryable();
                
            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
            {
                customersQuery = customersQuery.Where(c => 
                    c.Name.Contains(searchString) || 
                    (c.Email != null && c.Email.Contains(searchString)) ||
                    (c.Phone != null && c.Phone.Contains(searchString)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(searchString)));
            }
            
            // Durum filtresi
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                customersQuery = customersQuery.Where(c => c.IsActive == isActive);
            }
            
            // Müşteri tipi filtresi
            if (customerType.HasValue)
            {
                customersQuery = customersQuery.Where(c => c.Type == customerType.Value);
            }
            
            // Seçim listeleri için verileri hazırla
            ViewData["CustomerTypes"] = new SelectList(Enum.GetValues(typeof(CustomerType))
                .Cast<CustomerType>()
                .Select(c => new { Id = (int)c, Name = GetDisplayName(c) }), "Id", "Name");
                
            ViewData["SearchString"] = searchString;
            ViewData["Status"] = status;
            ViewData["CustomerType"] = customerType;
            
            var customers = await customersQuery.ToListAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewData["CustomerTypes"] = new SelectList(Enum.GetValues(typeof(CustomerType))
                .Cast<CustomerType>()
                .Select(c => new { Id = (int)c, Name = GetDisplayName(c) }), "Id", "Name");
                
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Name,TaxNumber,Email,Phone,Address,Type,Notes,IsActive")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.CreatedAt = DateTime.Now;
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Müşteri '{customer.Name}' başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Müşteri oluşturulurken bir hata oluştu: " + ex.Message);
                    LogError(ex, "Müşteri oluşturma hatası");
                }
            }
            
            ViewData["CustomerTypes"] = new SelectList(Enum.GetValues(typeof(CustomerType))
                .Cast<CustomerType>()
                .Select(c => new { Id = (int)c, Name = GetDisplayName(c) }), "Id", "Name");
                
            return View(customer);
        }

        // GET: Customers/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            
            ViewData["CustomerTypes"] = new SelectList(Enum.GetValues(typeof(CustomerType))
                .Cast<CustomerType>()
                .Select(c => new { Id = (int)c, Name = GetDisplayName(c) }), "Id", "Name");
                
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TaxNumber,Email,Phone,Address,Type,Notes,IsActive,CreatedAt")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Müşteri '{customer.Name}' başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Müşteri güncellenirken eşzamanlılık hatası oluştu: " + ex.Message);
                        LogError(ex, "Müşteri güncelleme eşzamanlılık hatası");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Müşteri güncellenirken bir hata oluştu: " + ex.Message);
                    LogError(ex, "Müşteri güncelleme hatası");
                }
            }
            
            ViewData["CustomerTypes"] = new SelectList(Enum.GetValues(typeof(CustomerType))
                .Cast<CustomerType>()
                .Select(c => new { Id = (int)c, Name = GetDisplayName(c) }), "Id", "Name");
                
            return View(customer);
        }

        // GET: Customers/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (customer == null)
            {
                return NotFound();
            }

            // İlişkili projeler var mı kontrol et
            var hasProjects = customer.Projects != null && customer.Projects.Any();
            ViewBag.HasProjects = hasProjects;

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (customer == null)
            {
                return NotFound();
            }

            // İlişkili projeler var mı kontrol et
            if (customer.Projects != null && customer.Projects.Any())
            {
                TempData["ErrorMessage"] = $"Müşteri '{customer.Name}' silinemiyor çünkü müşteriye bağlı projeler var. Önce ilişkili projeleri silmelisiniz.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Müşteri '{customer.Name}' başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Müşteri silinirken bir hata oluştu: {ex.Message}";
                LogError(ex, "Müşteri silme hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
        
        private string GetDisplayName(CustomerType customerType)
        {
            var displayAttribute = typeof(CustomerType)
                .GetField(customerType.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
                
            return displayAttribute?.Name ?? customerType.ToString();
        }
        
        private void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, "{Message}", message);
        }
    }
} 