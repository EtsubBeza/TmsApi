using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/assessments")]
public class AssessmentsController(
    IAssessmentService assessmentService) : ControllerBase
{

    // GET: api/assessments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var assessments = await assessmentService.GetAllAsync();

        return Ok(assessments);
    }



    // GET: api/assessments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {

        var assessment = await assessmentService.GetByIdAsync(id);


        return assessment is not null
            ? Ok(assessment)
            : NotFound();

    }




    // POST: api/assessments
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssessmentRequest request)
    {


        var assessment = await assessmentService.CreateAsync(

            request.Title,

            request.MaxScore,

            request.Weight,

            request.CourseId

        );



        return CreatedAtAction(

            nameof(GetById),

            new { id = assessment.Id },

            assessment

        );

    }





    // DELETE: api/assessments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {


        var deleted = await assessmentService.DeleteAsync(id);



        return deleted

            ? NoContent()

            : NotFound();

    }





    public record CreateAssessmentRequest(

        string Title,

        decimal MaxScore,

        decimal Weight,

        int CourseId

    );
}