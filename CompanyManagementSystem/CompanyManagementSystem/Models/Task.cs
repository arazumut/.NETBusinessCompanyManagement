using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementSystem.Models
{
    public class WorkTask
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Görev adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Görev adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Görev Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; }

        // Proje ile ilişki
        [Display(Name = "Proje")]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        // Atanan personel ile ilişki
        [Display(Name = "Atanan Personel")]
        public int? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Employee AssignedTo { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Durum")]
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.ToDo;

        [Range(0, 100, ErrorMessage = "Tamamlanma yüzdesi 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "Tamamlanma Yüzdesi")]
        public int CompletionPercentage { get; set; }

        [Display(Name = "Öncelik")]
        public Priority Priority { get; set; } = Priority.Medium;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Son Güncelleme")]
        public DateTime? UpdatedAt { get; set; }
    }

    public enum WorkTaskStatus
    {
        [Display(Name = "Yapılacak")]
        ToDo = 1,
        
        [Display(Name = "Devam Ediyor")]
        InProgress = 2,
        
        [Display(Name = "Tamamlandı")]
        Done = 3
    }

    public enum Priority
    {
        [Display(Name = "Düşük")]
        Low = 1,
        
        [Display(Name = "Orta")]
        Medium = 2,
        
        [Display(Name = "Yüksek")]
        High = 3,
        
        [Display(Name = "Acil")]
        Urgent = 4
    }
} 