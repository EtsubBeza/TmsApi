using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {

        builder.Property<DateTime>("LastUpdated")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasKey(s => s.Id);

builder.HasQueryFilter(s => !s.IsDeleted);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);


        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(s => s.GPA)
            .HasPrecision(3, 2);


        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

    }
}