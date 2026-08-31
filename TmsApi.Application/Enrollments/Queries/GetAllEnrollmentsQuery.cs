using MediatR;

namespace TmsApi.Application.Enrollments.Queries;

public record GetAllEnrollmentsQuery
    : IRequest<List<EnrollmentListItemDto>>;

public record EnrollmentListItemDto(
    string Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    string EnrolledAt);