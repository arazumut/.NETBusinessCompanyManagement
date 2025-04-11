using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CompanyManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Müşteri adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Müşteri Adı")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Vergi numarası en fazla 100 karakter olabilir.")]
        [Display(Name = "Vergi Numarası")]
        public string? TaxNumber { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Adres")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Müşteri Olma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Müşteri Tipi")]
        public CustomerType Type { get; set; } = CustomerType.Company;

        [Display(Name = "Notlar")]
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
        public string? Notes { get; set; }

        // İlişkiler
        public virtual ICollection<Project>? Projects { get; set; }
    }

    public enum CustomerType
    {
        [Display(Name = "Şirket")]
        Company = 1,
        
        [Display(Name = "Bireysel")]
        Individual = 2,
        
        [Display(Name = "Kamu Kurumu")]
        Government = 3,
        
        [Display(Name = "Diğer")]
        Other = 4
    }
} 