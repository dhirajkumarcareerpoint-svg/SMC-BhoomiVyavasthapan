namespace SMC.Application.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string? MessageMr { get; set; }   // मराठी संदेश
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? messageMr = null) =>
        new() { Success = true, Data = data, MessageMr = messageMr };

    public static ApiResponse<T> Fail(string messageMr, List<string>? errors = null) =>
        new() { Success = false, MessageMr = messageMr, Errors = errors };
}
