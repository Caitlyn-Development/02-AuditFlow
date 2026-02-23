using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _02_AuditFlowApplication.Models
{
    public class Audit
    {
        [Key] public int AuditId { get; set; }

        [Required][MaxLength(200)] public string AuditName { get; set; }

        [Required] public AuditType Type { get; set; }

        [Required] public DateTime StartDate { get; set; }

        [Required] public DateTime EndDate { get; set; }

        public bool IsRecurring { get; set; }

        public RecurrenceFrequency? RecurrenceType { get; set; }

        [Required] public AuditStatus Status { get; set; } = AuditStatus.NotStarted;

        [ForeignKey("CreatedBy")] public int CreatedByUserID  { get; set; }

        public virtual User CreatedBy {  get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public enum AuditType
    {
        Security = 1,
        Safety = 2,
        Quality = 3,
        DataProtection = 4,
        Financial = 5,
        Unknown = 6
    }

    public enum AuditStatus
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        Overdue = 4
    }

    public enum RecurrenceFrequency
    {
        Weekly = 1,
        Monthly = 2,
        Quarterly = 3,
        Annual = 4
    }
}
