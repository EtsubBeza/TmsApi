namespace TmsApi.Records;

public record StudentRecord(
    string Id,
    string Name,
    DateTime EnrollmentDate,
    double? Gpa);