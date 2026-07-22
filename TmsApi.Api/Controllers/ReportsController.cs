using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{

    // 1. Count active students with GPA >= 3.0
    [HttpGet("active-high-gpa")]
    public async Task<IActionResult> ActiveHighGpa()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(count);
    }




    // 2. Courses with most enrollments
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> CourseEnrollments()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();


        return Ok(list);
    }




    // 3. Average GPA per course
    [HttpGet("course-average-gpa")]
    public async Task<IActionResult> CourseAverageGpa()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();


        return Ok(list);
    }




    // 4. Students with zero enrollments
    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> StudentsWithoutEnrollments()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();


        return Ok(list);
    }






    // EXERCISE 3 - TODO 1
    // Pagination
    // page size = 20
    // OrderBy -> Skip -> Take
  [HttpGet("students-page")]
public async Task<IActionResult> StudentsPage(
    int page,
    CancellationToken cancellationToken)
{

    if (page < 1)
    {
        page = 1;
    }


    int pageSize = 20;


    var students = await context.Students

        .OrderBy(s => s.Name)

        .Skip((page - 1) * pageSize)

        .Take(pageSize)

        .ToListAsync(cancellationToken);


    return Ok(students);
}



    // EXERCISE 3 - TODO 2
    // Top 5 courses by enrollment count
    [HttpGet("top-courses")]
    public async Task<IActionResult> TopCourses(
        CancellationToken cancellationToken)
    {


        var courses = await context.Courses

            .GroupBy(c => c.Title)

            .Select(g => new
            {
                CourseTitle = g.Key,

                EnrollmentCount = g.Count()
            })

            .OrderByDescending(x => x.EnrollmentCount)

            .Take(5)

            .ToListAsync(cancellationToken);



        return Ok(courses);
    }

}
