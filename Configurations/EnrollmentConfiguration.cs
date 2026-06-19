using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {

        builder.HasKey(e => e.Id);



        builder.Property(e => e.Grade)
            .HasPrecision(5, 2);



        builder.Property(e => e.EnrolledAt)
            .IsRequired();




        // Student can have many enrollments.
        // Deleting a student deletes their enrollments.
        builder.HasOne(e => e.Student)

            .WithMany(s => s.Enrollments)

            .HasForeignKey(e => e.StudentId)

            .OnDelete(DeleteBehavior.Cascade);





        // Course can have many enrollments.
        // Restrict prevents deleting a course that still has students enrolled.
        builder.HasOne(e => e.Course)

            .WithMany(c => c.Enrollments)

            .HasForeignKey(e => e.CourseId)

            .OnDelete(DeleteBehavior.Restrict);

    }
}