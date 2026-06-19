using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }


    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");

        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");

            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }
}