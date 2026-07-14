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

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
    PagedRequest request,
    CancellationToken ct)
{
    IQueryable<Student> query = _context.Students
        .AsNoTracking()
        .Where(s => !s.IsDeleted);


    // Search filter
    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        query = query.Where(s =>
            EF.Functions.ILike(
                s.Name,
                $"%{request.Search}%")
            ||
            EF.Functions.ILike(
                s.RegistrationNumber,
                $"%{request.Search}%"));
    }


    // Count BEFORE pagination
    var totalCount = await query.CountAsync(ct);


    // Sorting
    query = request.OrderBy switch
    {
        "RegistrationNumber" =>
            request.Descending
            ? query.OrderByDescending(s => s.RegistrationNumber)
            : query.OrderBy(s => s.RegistrationNumber),


        "GPA" =>
            request.Descending
            ? query.OrderByDescending(s => s.GPA)
            : query.OrderBy(s => s.GPA),


        _ =>
            request.Descending
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name)
    };


    var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(s => new StudentResponseDto(
            s.Id,
            s.RegistrationNumber,
            s.Name,
            (double)s.GPA,
            s.IsActive))
        .ToListAsync(ct);


    return new PagedResponse<StudentResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
}