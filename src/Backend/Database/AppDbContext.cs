using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    
    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Attendance> Attendances => Set<Attendance>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Camera> Cameras => Set<Camera>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<CameraApiKey> CameraApiKeys => Set<CameraApiKey>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(configuration["DatabaseSchema"] ?? "public");
    }
}