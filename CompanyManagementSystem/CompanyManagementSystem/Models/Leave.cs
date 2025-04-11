using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementSystem.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }

        // Personel ile ilişki
        [Required(ErrorMessage = "Personel seçimi zorunludur.")]
        [Display(Name = "Personel")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required(ErrorMessage = "İzin türü zorunludur.")]
        [Display(Name = "İzin Türü")]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Description { get; set; }

        [Display(Name = "Durum")]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [Display(Name = "Talep Tarihi")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Display(Name = "İşlem Tarihi")]
        public DateTime? ActionDate { get; set; }

        [Display(Name = "İşlem Açıklaması")]
        public string ActionReason { get; set; }

        [Display(Name = "İşlemi Yapan")]
        public string ActionBy { get; set; }
    }

    public enum LeaveType
    {
        [Display(Name = "Yıllık İzin")]
        Annual = 1,
        
        [Display(Name = "Hastalık")]
        Sick = 2,
        
        [Display(Name = "Mazeret")]
        Personal = 3,
        
        [Display(Name = "Doğum")]
        Maternity = 4,
        
        [Display(Name = "Babalık")]
        Paternity = 5,
        
        [Display(Name = "Evlilik")]
        Marriage = 6,
        
        [Display(Name = "Diğer")]
        Other = 7
    }

    public enum LeaveStatus
    {
        [Display(Name = "Bekliyor")]
        Pending = 1,
        
        [Display(Name = "Onaylandı")]
        Approved = 2,
        
        [Display(Name = "Reddedildi")]
        Rejected = 3
    }
} 