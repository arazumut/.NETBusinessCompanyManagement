using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CompanyManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 karakter olmalıdır.")]
        [Display(Name = "TC Kimlik No")]
        public string IdentityNumber { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }

        [Display(Name = "Adres")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string Address { get; set; }

        // Departman ile ilişki
        [Display(Name = "Departman")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        // Pozisyon ile ilişki
        [Display(Name = "Pozisyon")]
        public int PositionId { get; set; }

        [ForeignKey("PositionId")]
        public virtual Position Position { get; set; }

        [Required(ErrorMessage = "Başlama tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlama Tarihi")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Currency)]
        [Range(0, 1000000, ErrorMessage = "Maaş 0 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Maaş")]
        public decimal Salary { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        // Kullanıcı hesabı ile ilişki
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        // İzinler ile ilişki
        public virtual ICollection<Leave> Leaves { get; set; }

        // Projeler ile ilişki (isteğe bağlı)
        public virtual ICollection<Project> Projects { get; set; }
    }
} 