using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;


public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);

    Task<EnrollmentRecord?> GetByIdAsync(string id);

    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
}



public class EnrollmentService : IEnrollmentService
{

    private readonly TmsDbContext _context;

    private readonly ILogger<EnrollmentService> _logger;



    public EnrollmentService(
        TmsDbContext context,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }






    public async Task<EnrollmentRecord> EnrollAsync(
        string studentId,
        string courseCode)
    {


        var student = await _context.Students
            .FirstOrDefaultAsync(
                s => s.RegistrationNumber == studentId);



        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Code == courseCode);



        if(student == null || course == null)
        {
            throw new Exception(
                "Student or course not found");
        }





        // Duplicate check
        var existing = await _context.Enrollments

            .FirstOrDefaultAsync(e =>
                e.StudentId == student.Id &&
                e.CourseId == course.Id);



        if(existing != null)
        {

            _logger.LogWarning(
                "Duplicate enrollment {StudentId} in {CourseCode}",
                studentId,
                courseCode);



            return new EnrollmentRecord(
                existing.Id.ToString(),
                student.RegistrationNumber,
                course.Code,
                existing.EnrolledAt);

        }





        var enrollment = new Enrollment
        {

            StudentId = student.Id,

            CourseId = course.Id,

            EnrolledAt = DateTime.UtcNow

        };



        _context.Enrollments.Add(enrollment);


        await _context.SaveChangesAsync();




        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode}",
            studentId,
            courseCode);




        return new EnrollmentRecord(
            enrollment.Id.ToString(),
            student.RegistrationNumber,
            course.Code,
            enrollment.EnrolledAt);

    }







    public async Task<EnrollmentRecord?> GetByIdAsync(string id)
    {


        var enrollment = await _context.Enrollments

            .Include(e => e.Student)

            .Include(e => e.Course)

            .FirstOrDefaultAsync(
                e => e.Id.ToString() == id);




        if(enrollment == null)
        {

            _logger.LogWarning(
                "Enrollment {EnrollmentId} not found",
                id);


            return null;
        }





        return new EnrollmentRecord(
            enrollment.Id.ToString(),
            enrollment.Student.RegistrationNumber,
            enrollment.Course.Code,
            enrollment.EnrolledAt);

    }









    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {


        return await _context.Enrollments

            .Include(e => e.Student)

            .Include(e => e.Course)


            .Select(e => new EnrollmentRecord(

                e.Id.ToString(),

                e.Student.RegistrationNumber,

                e.Course.Code,

                e.EnrolledAt

            ))

            .ToListAsync();

    }









    public async Task<bool> DeleteAsync(string id)
    {


        var enrollment = await _context.Enrollments

            .FirstOrDefaultAsync(
                e => e.Id.ToString() == id);



        if(enrollment == null)
        {

            _logger.LogWarning(
                "Delete failed enrollment {EnrollmentId}",
                id);


            return false;
        }





        _context.Enrollments.Remove(enrollment);


        await _context.SaveChangesAsync();




        _logger.LogInformation(
            "Deleted enrollment {EnrollmentId}",
            id);



        return true;

    }

}





public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);