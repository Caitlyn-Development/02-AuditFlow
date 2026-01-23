using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;
using System.Security.RightsManagement;
using System.Text;

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

        public enum UserRole
        {
            Auditor = 1,
            Manager = 2
        }
    }
    } 
