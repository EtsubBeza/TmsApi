using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler(
    IEnrollmentService enrollmentService)
    : IRequestHandler<ApproveEnrollmentCommand, bool>
{
    public async Task<bool> Handle(
        ApproveEnrollmentCommand command,
        CancellationToken ct)
    {
        return await enrollmentService.ApproveAsync(
            command.EnrollmentId,
            ct);
    }
}