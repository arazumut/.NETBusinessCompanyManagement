using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CompanyManagementSystem.Models;

namespace CompanyManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Personel için DbSet
        public DbSet<Employee> Employees { get; set; }
        
        // Departman için DbSet
        public DbSet<Department> Departments { get; set; }
        
        // Pozisyon için DbSet
        public DbSet<Position> Positions { get; set; }
        
        // İzin için DbSet
        public DbSet<Leave> Leaves { get; set; }
        
        // Proje için DbSet (isteğe bağlı)
        public DbSet<Project> Projects { get; set; }
        
        // Görev için DbSet (isteğe bağlı)
        public DbSet<WorkTask> WorkTasks { get; set; }
        
        // Müşteri için DbSet
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Model yapılandırmaları ve ilişkiler burada tanımlanabilir
        }
    }
} 