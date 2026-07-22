using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;
public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TmsDbContext).Assembly);

}



    public DbSet<Student> Students { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Enrollment> Enrollments { get; set; }


    public DbSet<Assessment> Assessments { get; set; }

    public DbSet<Certificate> Certificates { get; set; }
}

