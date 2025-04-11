using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CompanyManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using CompanyManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Dashboard()
    {
        // Özet istatistikleri hesapla
        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.Where(e => e.IsActive).CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();
        var totalProjects = await _context.Projects.CountAsync();
        var activeProjects = await _context.Projects.Where(p => p.Status != ProjectStatus.Completed).CountAsync();
        var pendingLeaves = await _context.Leaves.Where(l => l.Status == LeaveStatus.Pending).CountAsync();
        
        // En son işe alınan çalışanlar
        var recentEmployees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .OrderByDescending(e => e.StartDate)
            .Take(5)
            .ToListAsync();
            
        // Devam eden projeler
        var ongoingProjects = await _context.Projects
            .Where(p => p.Status == ProjectStatus.InProgress)
            .OrderByDescending(p => p.StartDate)
            .Take(5)
            .ToListAsync();
            
        // Departman bazında çalışan sayıları
        var departmentStats = await _context.Departments
            .Select(d => new {
                DepartmentName = d.Name,
                EmployeeCount = d.Employees.Count
            })
            .OrderByDescending(x => x.EmployeeCount)
            .ToListAsync();
            
        // Dashboard veri modelini oluştur
        ViewData["TotalEmployees"] = totalEmployees;
        ViewData["ActiveEmployees"] = activeEmployees;
        ViewData["TotalDepartments"] = totalDepartments;
        ViewData["TotalProjects"] = totalProjects;
        ViewData["ActiveProjects"] = activeProjects;
        ViewData["PendingLeaves"] = pendingLeaves;
        ViewData["RecentEmployees"] = recentEmployees;
        ViewData["OngoingProjects"] = ongoingProjects;
        ViewData["DepartmentStats"] = departmentStats;
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
