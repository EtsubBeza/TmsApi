using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
using TmsApi.Interfaces;

namespace TmsApi.Services;

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

    public async Task<StudentResponseDto?> GetByIdAsync(string id)
    {
        return await _context.Students
            .Where(s => s.Id.ToString() == id)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                (double)s.GPA,
                s.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<StudentResponseDto>> GetAllAsync()
    {
        return await _context.Students
            .Where(s => !s.IsDeleted)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                (double)s.GPA,
                s.IsActive))
            .ToListAsync();
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request)
    {
        var student = new Student
        {
            RegistrationNumber = "TMS-" + Guid.NewGuid().ToString("N")[..8],
            Name = request.Name,
            GPA = (decimal)(request.Gpa ?? 0),
            IsActive = true,
            IsDeleted = false
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return new StudentResponseDto(
            student.Id,
            student.RegistrationNumber,
            student.Name,
            (double)student.GPA,
            student.IsActive);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id.ToString() == id);

        if (student is null)
            return false;

        student.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }
}