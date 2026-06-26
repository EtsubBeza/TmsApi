namespace TmsApi.Records;

public record CourseRecord(
    string Id,
    string Title,
    int Capacity,
    DateTime CreatedAt);