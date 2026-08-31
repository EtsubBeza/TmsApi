using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    // Old M6 read endpoints
    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);

    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);

    // Old M6 create endpoint
    // Keep for now if other code uses it
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct);

    // CQRS methods
    Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct);

    Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct);

    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct);

    // Get all enrollments for Module 9 SignalStore
    Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct);

    // Approve enrollment
    Task<bool> ApproveAsync(
        int enrollmentId,
        CancellationToken ct);
}
