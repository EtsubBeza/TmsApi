using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Interfaces;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
public async Task<IActionResult> GetStudents(
    [FromQuery] PagedRequest request,
    CancellationToken ct)
{
    var result = await studentService.GetStudentsAsync(request, ct);

    return Ok(result);
}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);
        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = student.Id },
            student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}