using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Attendance> Attendances => Set<Attendance>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Camera> Cameras => Set<Camera>();
    
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    
    public DbSet<CameraApiKey> CameraApiKeys => Set<CameraApiKey>();

    public DbSet<Admin> Admins => Set<Admin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(configuration["DatabaseSchema"] ?? "public");
        
        modelBuilder.Entity<Admin>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Student>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<Camera>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<Teacher>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Student>()
            .HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId);

        modelBuilder.Entity<Course>()
            .HasKey(c => new { c.Code, c.Year, c.SemesterCode });

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Classes)
            .WithOne(cl => cl.Course)
            .HasForeignKey(cl => new { cl.CourseCode, cl.CourseYearId, cl.CourseSemesterCode });

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => new { e.CourseCode, e.CourseYearId, e.CourseSemesterCode });

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
            .HasForeignKey(a => a.StudentId);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Class)
            .WithMany()
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