
namespace TmsApi.Dtos;

public record PagedRequest
{
    private const int MaxPageSize = 50;

    private int _pageSize = 20;

    public int Page { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;

        init
        {
            if (value < 1)
                _pageSize = 20;
            else if (value > MaxPageSize)
                _pageSize = MaxPageSize;
            else
                _pageSize = value;
        }
    }

    public string? Search { get; init; }

    public string OrderBy { get; init; } = "Title";

    public bool Descending { get; init; }
}