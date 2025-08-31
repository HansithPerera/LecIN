using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Flags]
public enum AdminPermissions
{
    None = 0,
    ManageUsers = 1 << 0,
    ManageCourses = 1 << 1,
    ViewReports = 1 << 2,
    FullAccess = ManageUsers | ManageCourses | ViewReports
}

[Table("Admins")]
public class Admin
{
    [MaxLength(255)] [Key] public required string Id { get; set; }

    [MaxLength(255)] public required string FirstName { get; set; }

    [MaxLength(255)] public required string LastName { get; set; }

    [MaxLength(255)] public required string Email { get; set; }

    public AdminPermissions Permissions { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
}