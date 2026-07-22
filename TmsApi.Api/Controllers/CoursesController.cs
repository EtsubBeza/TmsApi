using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[ApiExplorerSettings(IgnoreApi = true)]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{


    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);

        return Ok(result);
    }




    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {

        var course = await courseService.GetByIdAsync(id, ct);


        if (course is null)
            return NotFound();



        var links = new List<LinkDto>
        {
            new(
                linkGenerator.GetPathByName(
                    HttpContext,
                    nameof(GetCourseById),
                    new { id })!,
                "self",
                "GET"),


            new(
                linkGenerator.GetPathByName(
                    HttpContext,
                    nameof(GetCourseById),
                    new { id })!,
                "update",
                "PUT"),


            new(
                linkGenerator.GetPathByName(
                    HttpContext,
                    nameof(GetCourseById),
                    new { id })!,
                "delete",
                "DELETE"),


            // New CQRS enrollment endpoint
            new(
                "/api/v2/enrollments",
                "enrollments",
                "POST")
        };



        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(
                new LinkDto(
                    "/api/v2/enrollments",
                    "enroll",
                    "POST"));
        }



        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };


        return Ok(detail);
    }





    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }



        var result =
            await courseService.CreateAsync(request, ct);



        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }
}