using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laborator4_AI.Domain.Models
{
    [Table("Grade")]
    public class StudentGradeEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GradeId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Exam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Activity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Final { get; set; }

        // Navigation property
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
}