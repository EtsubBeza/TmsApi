using TmsApi.Records;

namespace TmsApi.Interfaces;

public interface IAssessmentService
{
    Task<AssessmentRecord> CreateAsync(
        string title,
        decimal maxScore,
        decimal weight,
        int courseId);

    Task<AssessmentRecord?> GetByIdAsync(string id);

    Task<IReadOnlyList<AssessmentRecord>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
}