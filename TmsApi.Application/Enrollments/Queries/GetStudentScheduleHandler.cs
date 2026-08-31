using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetAllEnrollmentsHandler(
    IEnrollmentService enrollmentService)
    : IRequestHandler<GetAllEnrollmentsQuery, List<EnrollmentListItemDto>>
{
    public async Task<List<EnrollmentListItemDto>> Handle(
        GetAllEnrollmentsQuery query,
        CancellationToken ct)
    {
        var enrollments =
            await enrollmentService.GetAllAsync(ct);

        return enrollments
            .Select(e => new EnrollmentListItemDto(
                e.Id.ToString(),
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.IsArchived ? "Rejected" : "Pending",
                e.EnrolledAt.ToString("O")))
            .ToList();
    }
}