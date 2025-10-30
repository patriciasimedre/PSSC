using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laborator4_AI.Domain.Models
{
    [Table("Student")]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        [StringLength(7)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<StudentGradeEntity> Grades { get; set; } = new List<StudentGradeEntity>();
    }
}