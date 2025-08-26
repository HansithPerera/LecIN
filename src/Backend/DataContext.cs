using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
}