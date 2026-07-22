using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
using TmsApi.Records;

namespace TmsApi.Infrastructure.Persistence.Services;

public class AssessmentService : IAssessmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<AssessmentService> _logger;

    public AssessmentService(
        TmsDbContext context,
        ILogger<AssessmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssessmentRecord> CreateAsync(
        string title,
        decimal maxScore,
        decimal weight,
        int courseId)
    {
        var existing = await _context.Assessments
            .FirstOrDefaultAsync(a =>
                a.Title == title &&
                a.CourseId == courseId);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate assessment {Title} already exists",
                title);

            return new AssessmentRecord(
                existing.Id.ToString(),
                existing.Title,
                existing.MaxScore,
                existing.Weight,
                existing.CourseId);
        }

        var assessment = new Assessment
        {
            Title = title,
            MaxScore = maxScore,
            Weight = weight,
            CourseId = courseId
        };

        _context.Assessments.Add(assessment);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created assessment {Title}",
            title);

        return new AssessmentRecord(
            assessment.Id.ToString(),
            assessment.Title,
            assessment.MaxScore,
            assessment.Weight,
            assessment.CourseId);
    }

    public async Task<AssessmentRecord?> GetByIdAsync(string id)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id.ToString() == id);

        if (assessment is null)
        {
            _logger.LogWarning(
                "Assessment {AssessmentId} not found",
                id);

            return null;
        }

        return new AssessmentRecord(
            assessment.Id.ToString(),
            assessment.Title,
            assessment.MaxScore,
            assessment.Weight,
            assessment.CourseId);
    }

    public async Task<IReadOnlyList<AssessmentRecord>> GetAllAsync()
    {
        return await _context.Assessments
            .Select(a => new AssessmentRecord(
                a.Id.ToString(),
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id.ToString() == id);

        if (assessment is null)
        {
            _logger.LogWarning(
                "Delete failed. Assessment {AssessmentId} not found",
                id);

            return false;
        }

        _context.Assessments.Remove(assessment);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted assessment {AssessmentId}",
            id);

        return true;
    }
}
