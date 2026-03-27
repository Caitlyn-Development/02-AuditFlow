using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _02_AuditFlowApplication.Models
{
    public class AuditTask
    {
        [Key] public int TaskId { get; set; }

        [Required][MaxLength(200)] public string TaskName { get; set; }

        public string Description { get; set; }

        [Required][ForeignKey("Audit")] public int AuditId { get; set; }

        [Required][MaxLength(200)] public string AuditName { get; set; }

        [Required][ForeignKey("AssignedToUser")] public int AssignedToUserId { get; set; }

        public virtual User AssignedToUser { get; set; }

        [Required] public DateTime DueDate { get; set; }

        [Required] public AuditTaskStatus Status { get; set; } = AuditTaskStatus.NotStarted;

        [NotMapped]
        public string StatusDisplay => Status switch
        {
            AuditTaskStatus.NotStarted => "Not Started",
            AuditTaskStatus.InProgress => "In Progress",
            AuditTaskStatus.OnHold => "On Hold",
            AuditTaskStatus.Completed => "Completed",
            AuditTaskStatus.Overdue => "Overdue",
            _ => "Unknown"
        };

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [NotMapped] public int EvidenceCount { get; set; }

        [NotMapped]
        public string EvidenceCountDisplay => EvidenceCount == 0
            ? "None"
            : EvidenceCount == 1
            ? "1 file"
            : $"{EvidenceCount} files";
    }

    public enum AuditTaskStatus
    {
        NotStarted = 1,
        InProgress = 2,
        OnHold = 3,
        Completed = 4,
        Overdue = 5
    }
}
