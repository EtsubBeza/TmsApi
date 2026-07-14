using TmsApi.Dtos;

namespace TmsApi.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(string id);

    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request);

    Task<bool> DeleteAsync(string id);
}