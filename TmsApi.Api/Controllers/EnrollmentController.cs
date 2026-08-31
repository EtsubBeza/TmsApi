
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
[Tags("Enrollments")]
public class EnrollmentController(
    IMediator mediator)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentCreated),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(
        [FromBody] EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created),

            error =>
                Problem(
                    statusCode: error.Code switch
                    {
                        "course_not_found" => 404,
                        "course_full" => 409,
                        "already_enrolled" => 409,
                        _ => 400
                    },
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type:
                        $"https://tms.local/errors/{error.Code}"
                )
        );
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<EnrollmentListItemDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var result =
            await mediator.Send(
                new GetAllEnrollmentsQuery(),
                ct);

        return Ok(result);
    }

    [HttpGet("{studentId:int}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var result =
            await mediator.Send(
                new GetStudentScheduleQuery(studentId),
                ct);

        return Ok(result);
    }

    [HttpPost("{enrollmentId:int}/approve")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> Approve(
    int enrollmentId,
    CancellationToken ct)
{
    var approved = await mediator.Send(
        new ApproveEnrollmentCommand(enrollmentId),
        ct);

    if (approved)
    {
        return Ok();
    }

    return Conflict(new
    {
        message = "Enrollment could not be approved."
    });
}
}

