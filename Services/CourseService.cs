using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;


public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string title, int capacity);

    Task<CourseRecord?> GetByIdAsync(string id);

    Task<IReadOnlyList<CourseRecord>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
}




public class CourseService : ICourseService
{

    private readonly TmsDbContext _context;

    private readonly ILogger<CourseService> _logger;



    public CourseService(
        TmsDbContext context,
        ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }






    public async Task<CourseRecord> CreateAsync(
        string title,
        int capacity)
    {

        var existing = await _context.Courses
            .FirstOrDefaultAsync(c => c.Title == title);



        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate course {CourseTitle} already exists",
                title);


            return new CourseRecord(
                existing.Id.ToString(),
                existing.Title,
                existing.Capacity,
                DateTime.UtcNow);
        }





        var course = new Course
        {
            Code = Guid.NewGuid()
                .ToString("N")[..8],

            Title = title,

            Capacity = capacity
        };



        _context.Courses.Add(course);


        await _context.SaveChangesAsync();



        _logger.LogInformation(
            "Created course {CourseTitle}",
            title);



        return new CourseRecord(
            course.Id.ToString(),
            course.Title,
            course.Capacity,
            DateTime.UtcNow);
    }







    public async Task<CourseRecord?> GetByIdAsync(string id)
    {

        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Id.ToString() == id);



        if(course is null)
        {
            _logger.LogWarning(
                "Course {CourseId} not found",
                id);

            return null;
        }




        return new CourseRecord(
            course.Id.ToString(),
            course.Title,
            course.Capacity,
            DateTime.UtcNow);
    }








    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {

        return await _context.Courses

            .Select(c => new CourseRecord(
                c.Id.ToString(),
                c.Title,
                c.Capacity,
                DateTime.UtcNow))

            .ToListAsync();

    }









    public async Task<bool> DeleteAsync(string id)
    {

        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Id.ToString() == id);



        if(course is null)
        {

            _logger.LogWarning(
                "Delete failed. Course {CourseId} not found",
                id);


            return false;
        }




        _context.Courses.Remove(course);


        await _context.SaveChangesAsync();



        _logger.LogInformation(
            "Deleted course {CourseId}",
            id);



        return true;
    }

}





public record CourseRecord(
    string Id,
    string Title,
    int Capacity,
    DateTime CreatedAt);