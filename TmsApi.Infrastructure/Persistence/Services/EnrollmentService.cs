using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Persistence.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger)
    : IEnrollmentService
{
    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in Course {CourseId}",
            request.StudentId,
            courseId);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .ToListAsync(ct);
    }

    // CQRS methods

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AnyAsync(
                e => e.StudentId == studentId &&
                     e.Course.Code == courseCode,
                ct);
    }

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrollment created Student:{StudentId} Course:{CourseId}",
            enrollment.StudentId,
            enrollment.CourseId);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // Module 9 - Get all enrollments for SignalStore
    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsNoTracking()
            .Where(e => !e.IsArchived)
            .ToListAsync(ct);
    }
    
    // Approve enrollment
    public async Task<bool> ApproveAsync(
        int enrollmentId,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e => e.Id == enrollmentId,
                ct);

        if (enrollment is null)
        {
            return false;
        }

        enrollment.Status = "Approved";

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrollment {EnrollmentId} approved",
            enrollmentId);

        return true;
    }

}