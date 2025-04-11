using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementSystem.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Departman adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Departman adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Departman Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Description { get; set; }

        // İlişkiler
        public virtual ICollection<Position> Positions { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
} 