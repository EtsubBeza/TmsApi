namespace TmsApi.Records;

public record AssessmentRecord(
    string Id,
    string Title,
    decimal MaxScore,
    decimal Weight,
    int CourseId);