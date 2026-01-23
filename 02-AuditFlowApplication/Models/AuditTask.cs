using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _02_AuditFlowApplication.Models
{
    public class AuditTask
    {
        [Key] public int TaskId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; }

        public string Description { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }
        public virtual Audit Audit { get; set; }

        [Required]
        [ForeignKey("AssignedTo")]
        public int AssignedToUserId { get; set; }
        public virtual User AssignedTo { get; set; }

        [Required] public DateTime DueDate { get; set; }

        [Required] public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public enum TaskStatus
    {
        NotStarted = 1,
        InProgress = 2,
        OnHold = 3,
        Completed = 4
    }
}
