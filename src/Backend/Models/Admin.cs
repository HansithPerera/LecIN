using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Flags]
public enum AdminPermissions
{
    None = 0,
    FullAccess = ManageCourses | ManageReports | ManageTeachers | ManageStudents | ExportData,

    #region Export

    ExportData = 1 << 0,

    #endregion

    #region Write

    ManageCourses = (1 << 1) | ReadCourses,
    ManageReports = (1 << 2) | ReadReports,
    ManageTeachers = (1 << 3) | ReadTeachers,
    ManageStudents = (1 << 4) | ReadStudents,

    #endregion

    #region Read

    ReadTeachers = 1 << 5,
    ReadStudents = 1 << 6,
    ReadCourses = 1 << 7,
    ReadReports = 1 << 8,

    #endregion
}

[Table("Admins")]
public class Admin
{
    [MaxLength(255)] public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)] public required string FirstName { get; set; }

    [MaxLength(255)] public required string LastName { get; set; }

    [MaxLength(255)] public required string Email { get; set; }

    public AdminPermissions Permissions { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}