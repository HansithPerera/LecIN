//database pw: enwrrSMtnKE2xPGk

//post check & user secrets
// cmd in \src\backend

//user secrets
/* 
$serviceKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1heGFkdXhzZW5mcmJnb21maHBpIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjE3NTcwNywiZXhwIjoyMDcxNzUxNzA3fQ.nVtagJJTcz_JgG2YGfDUpgNQGqrjv_ynXPIT6c15FWs"
$project    = "maxaduxsenfrbgomfhpi"
$studentId  = "49bf8c3d-f7c9-4b2c-862a-9477ddcff815"
$classId    = "1cd6309a-b07e-420e-8e94-945225db8d26"

Invoke-RestMethod -Method Delete `
 -Uri "https://$project.supabase.co/rest/v1/Attendance?StudentId=eq.$studentId&ClassId=eq.$classId" `
 -Headers @{ apikey=$serviceKey; Authorization="Bearer $serviceKey" }
*/

//post check
/*
 $body = @{ ClassId = "1cd6309a-b07e-420e-8e94-945225db8d26" } | ConvertTo-Json
Invoke-RestMethod -Method Post "http://localhost:5105/api/student/check-in" -ContentType "application/json" -Body $body
*/

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
        //modelBuilder.Entity<Class>(entity =>{entity.Property(e => e.CourseYearId).HasColumnName("CourseYear");});
    }
}