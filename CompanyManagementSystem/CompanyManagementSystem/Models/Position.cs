using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementSystem.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Pozisyon adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Pozisyon adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Pozisyon Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Description { get; set; }

        // Departman ile ilişki
        [Required(ErrorMessage = "Departman seçimi zorunludur.")]
        [Display(Name = "Departman")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        // Personel ile ilişki
        public virtual ICollection<Employee> Employees { get; set; }
    }
} 