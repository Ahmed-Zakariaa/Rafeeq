namespace Rafeeq.Domain.Common;

/// <summary>Single response envelope returned by every endpoint, so the FE has one contract.</summary>
public class ResultViewModel<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public static ResultViewModel<T> Success(T data, string? message = null) =>
        new() { Data = data, IsSuccess = true, Message = message };

    public static ResultViewModel<T> Paged(T data, int total, int pageNumber, int pageSize) =>
        new() { Data = data, IsSuccess = true, Total = total, PageNumber = pageNumber, PageSize = pageSize };

    public static ResultViewModel<T> Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}

/// <summary>Lightweight {Id, Name, IsActive} item for dropdowns / lookups.</summary>
public class LookupResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
