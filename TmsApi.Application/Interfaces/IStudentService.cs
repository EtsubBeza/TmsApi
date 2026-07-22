using TmsApi.Application.DTOs;
namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(string id);

    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request);

    Task<bool> DeleteAsync(string id);
}
