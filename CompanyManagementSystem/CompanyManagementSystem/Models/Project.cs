using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementSystem.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Proje adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Proje adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Proje Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Durum")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        // Müşteri ile ilişki
        [Display(Name = "Müşteri")]
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        // İlişkiler
        public virtual ICollection<Employee>? Employees { get; set; }
        public virtual ICollection<WorkTask>? Tasks { get; set; }
    }

    public enum ProjectStatus
    {
        [Display(Name = "Planlama")]
        Planning = 1,
        
        [Display(Name = "Devam Ediyor")]
        InProgress = 2,
        
        [Display(Name = "Tamamlandı")]
        Completed = 3,
        
        [Display(Name = "İptal Edildi")]
        Cancelled = 4,
        
        [Display(Name = "Beklemede")]
        OnHold = 5
    }
} 