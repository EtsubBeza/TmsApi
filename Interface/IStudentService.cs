using TmsApi.Dtos;

namespace TmsApi.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(string id);

    Task<IReadOnlyList<StudentResponseDto>> GetAllAsync();

    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request);

    Task<bool> DeleteAsync(string id);
}