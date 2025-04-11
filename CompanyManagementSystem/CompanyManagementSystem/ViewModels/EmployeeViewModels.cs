using System;
using System.ComponentModel.DataAnnotations;
using CompanyManagementSystem.Models;

namespace CompanyManagementSystem.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

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
        [Required(ErrorMessage = "Departman seçimi zorunludur.")]
        [Display(Name = "Departman")]
        public int DepartmentId { get; set; }

        // Pozisyon ile ilişki
        [Required(ErrorMessage = "Pozisyon seçimi zorunludur.")]
        [Display(Name = "Pozisyon")]
        public int PositionId { get; set; }

        [Required(ErrorMessage = "Başlama tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlama Tarihi")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Maaş zorunludur.")]
        [DataType(DataType.Currency)]
        [Range(0, 1000000, ErrorMessage = "Maaş 0 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Maaş")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
} 