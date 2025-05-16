using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Document_Management_System.Models
{
    [Table("TaskAmount")]
    public class TaskAmount
    {
        [Key]
        public int Id { get; set; }

        public int AssignTaskId { get; set; }

        [Required]
        public int Amount { get; set; }

        public int? CurrentProgress { get; set; }

        [ForeignKey("AssignTaskId")]
        public AssignTask AssignTask { get; set; }
    }
}