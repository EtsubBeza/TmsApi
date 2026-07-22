using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateStudentRequest
{
    [Required]
    public string Name { get; init; } = default!;

    [Range(0, 4)]
    public double? Gpa { get; init; }
}
