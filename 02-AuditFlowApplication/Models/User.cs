using System.ComponentModel.DataAnnotations;

namespace _02_AuditFlowApplication.Models
{
    public class User
    {
        [Key] public int UserID { get; set; }

        [Required][MaxLength(50)] public string Username { get; set; }

        [Required] public string PasswordHash { get; set; }

        [Required][MaxLength(100)] public string FullName { get; set; }

        [Required] public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }

    public enum UserRole
    {
        Auditor = 1,
        Manager = 2
    }
} 
