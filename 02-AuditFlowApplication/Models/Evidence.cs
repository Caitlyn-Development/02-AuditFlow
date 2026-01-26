using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _02_AuditFlowApplication.Models
{
    public class Evidence
    {
        [Key] public int EvidenceId { get; set; }

        [Required][ForeignKey("Task")] public int TaskId { get; set; }

        public virtual AuditTask Task { get; set; }

        [Required][MaxLength(200)] public string FileName { get; set; }

        [Required] public string FilePath { get; set; }

        public long FileSize { get; set; }

        [Required][ForeignKey("SubmittedBy")] public int SubmittedByUserId { get; set; }

        public virtual User SubmittedBy { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Required] public EvidenceStatus Status { get; set; } = EvidenceStatus.PendingReview;

        [ForeignKey("ReviewedBy")] public int? ReviewedByUserId { get; set; }

        public virtual User ReviewedBy { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public string RejectionReason { get; set; }
    }

    public enum EvidenceStatus
    {
        PendingReview = 1,
        Approved = 2,
        Rejected = 3
    }
}
