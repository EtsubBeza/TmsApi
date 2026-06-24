using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;


public interface IStudentService
{
    Task<StudentRecord> CreateAsync(string name, double? gpa);

    Task<StudentRecord?> GetByIdAsync(string id);

    Task<IReadOnlyList<StudentRecord>> GetAllAsync();

    Task<IReadOnlyList<StudentRecord>> GetAllIncludingDeletedAsync();

    Task<bool> DeleteAsync(string id);
}




public class StudentService : IStudentService
{

    private readonly TmsDbContext _context;

    private readonly ILogger<StudentService> _logger;



    public StudentService(
        TmsDbContext context,
        ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }






    public async Task<StudentRecord> CreateAsync(
        string name,
        double? gpa)
    {


        var existing = await _context.Students

            .FirstOrDefaultAsync(
                s => s.Name == name);



        if(existing is not null)
        {

            _logger.LogWarning(
                "Duplicate student {StudentName} already exists",
                name);



            return new StudentRecord(

                existing.Id.ToString(),

                existing.Name,

                DateTime.UtcNow,

                (double?)existing.GPA

            );
        }






        var student = new Student
        {

            RegistrationNumber =
                "TMS-" + Guid.NewGuid()
                .ToString("N")[..8],


            Name = name,


            GPA = (decimal)(gpa ?? 0),


            IsActive = true,


            IsDeleted = false

        };





        _context.Students.Add(student);


        await _context.SaveChangesAsync();





        _logger.LogInformation(
            "Created student {StudentName} with id {StudentId}",
            name,
            student.Id);





        return new StudentRecord(

            student.Id.ToString(),

            student.Name,

            DateTime.UtcNow,

            (double?)student.GPA

        );

    }









    public async Task<StudentRecord?> GetByIdAsync(string id)
    {


        var student = await _context.Students

            .FirstOrDefaultAsync(
                s => s.Id.ToString() == id);





        if(student is null)
        {

            _logger.LogWarning(
                "Student {StudentId} not found",
                id);


            return null;
        }





        return new StudentRecord(

            student.Id.ToString(),

            student.Name,

            DateTime.UtcNow,

            (double?)student.GPA

        );

    }











    public async Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {


        return await _context.Students

            .Select(s => new StudentRecord(

                s.Id.ToString(),

                s.Name,

                DateTime.UtcNow,

                (double?)s.GPA

            ))

            .ToListAsync();

    }









    // Admin only - includes soft deleted students
    public async Task<IReadOnlyList<StudentRecord>> GetAllIncludingDeletedAsync()
    {


        return await _context.Students

            .IgnoreQueryFilters()

            .Select(s => new StudentRecord(

                s.Id.ToString(),

                s.Name,

                DateTime.UtcNow,

                (double?)s.GPA

            ))

            .ToListAsync();

    }









    // Soft delete
    public async Task<bool> DeleteAsync(string id)
    {


        var student = await _context.Students

            .IgnoreQueryFilters()

            .FirstOrDefaultAsync(
                s => s.Id.ToString() == id);





        if(student is null)
        {

            _logger.LogWarning(
                "Delete failed. Student {StudentId} not found",
                id);


            return false;

        }





        student.IsDeleted = true;



        await _context.SaveChangesAsync();





        _logger.LogInformation(
            "Soft deleted student {StudentId}",
            id);





        return true;

    }

}






public record StudentRecord(

    string Id,

    string Name,

    DateTime EnrollmentDate,

    double? Gpa);