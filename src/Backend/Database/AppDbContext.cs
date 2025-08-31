using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Attendance> Attendances => Set<Attendance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("private");

        modelBuilder.Entity<Course>()
            .HasKey(c => new { c.Code, c.Year, c.SemesterCode });

        modelBuilder.Entity<Class>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Class>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => new { c.CourseCode, c.CourseYearId, c.CourseSemesterCode });

        modelBuilder.Entity<Attendance>()
            .HasKey(a => new { a.StudentId, a.ClassId });

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .HasForeignKey(a => a.ClassId);

        modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.StudentId, e.CourseCode, e.CourseYearId, e.CourseSemesterCode });

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => new { e.CourseCode, e.CourseYearId, e.CourseSemesterCode })
            .HasForeignKey(e => e.StudentId);
    }
}