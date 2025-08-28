using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("private");
    }
    
    public DbSet<Student> Students => Set<Student>();
    
    public DbSet<Teacher> Teachers => Set<Teacher>();
    
    public DbSet<Class> Classes => Set<Class>();
    
    public DbSet<Course> Courses => Set<Course>();
    
    public DbSet<Attendance> Attendances => Set<Attendance>();
}