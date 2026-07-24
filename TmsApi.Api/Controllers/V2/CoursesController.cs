using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence.Services;

namespace TmsApi.Api.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    ICachedCourseService cachedCourseService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);


        var request = new PagedRequest
        {
            Page = page,
            PageSize = pageSize
        };


        var result =
            await cachedCourseService.GetCoursesAsync(
                request,
                ct);



        return Ok(new
        {
            data = result.Items,


            meta = new
            {
                result.TotalCount,

                result.Page,

                result.PageSize,

                result.TotalPages,

                result.HasNext,

                result.HasPrevious
            },


            links = new
            {
                self =
                    $"/api/v2/courses?page={page}&pageSize={pageSize}",


                next = result.HasNext
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : null,


                prev = result.HasPrevious
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : null,


                enroll = "/api/v2/enrollments"
            }
        });
    }
}